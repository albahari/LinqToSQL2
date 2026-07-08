using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReadWriteTests.SqlServer
{
	/// <summary>
	/// Tests for the "big join" nested-collection strategy (GroupJoin / correlated collection
	/// projections) against sources WITHOUT primary keys - views, heaps, TVFs.
	///
	/// Background: a big join reads 'count' consecutive rows per outer row from a single flat
	/// rowset, with no key check. That is only correct when the rowset is ordered so each outer
	/// row's joined rows are contiguous. The default ordering is synthesized from primary keys,
	/// so PK-less sources historically produced NO ORDER BY and silently corrupt groups
	/// (https://forum.linqpad.net/discussion/3276).
	///
	/// The fixture creates its own database (L2SBigJoinTests) with heap tables engineered so that
	/// SQL Server's join strategy returns rows interleaved across outer keys (clustered index on a
	/// non-key inner column => hash join => probe-order output). Every correctness test compares
	/// against a LINQ-to-Objects oracle computed from plainly-fetched rows, so no expected counts
	/// are hardcoded.
	///
	/// Test categories:
	///  - Correctness (oracle): must pass with ANY strategy (per-row fallback or ordered big join).
	///  - Strategy: assert WHICH strategy runs (single ordered big join vs fallback) - these pin
	///    the perf behavior and fail if the strategy silently changes.
	///  - Canary: proves the adversarial data layout still actually misorders an unordered join,
	///    i.e. that the correctness tests retain their teeth.
	/// </summary>
	[TestFixture]
	public class BigJoinOrderingTests
	{
		#region Entity definitions

		// Heap tables - no primary keys in the model (like a view or PK-less table).
		[Table (Name = "HeapOuter")]
		public class HeapOuter
		{
			[Column] public int Id { get; set; }
			[Column] public string Name { get; set; }
		}

		[Table (Name = "HeapInner")]
		public class HeapInner
		{
			[Column] public int IdOuter { get; set; }
			[Column] public string Tipo { get; set; }
			[Column] public int Amount { get; set; }
		}

		// Same physical tables, mapped WITH primary keys - the control group.
		[Table (Name = "PkOuter")]
		public class PkOuter
		{
			[Column (IsPrimaryKey = true)] public int Id { get; set; }
			[Column] public string Name { get; set; }
		}

		[Table (Name = "PkInner")]
		public class PkInner
		{
			[Column (IsPrimaryKey = true)] public int IdOuter { get; set; }
			[Column (IsPrimaryKey = true)] public string Tipo { get; set; }
			[Column] public int Amount { get; set; }
		}

		// PK'd outer with a PK-less (heap) inner - contiguity only needs outer ordering, so this
		// must big-join both before and after any change.
		[Table (Name = "HeapInner")]
		public class HeapInnerForPkOuter
		{
			[Column] public int IdOuter { get; set; }
			[Column] public string Tipo { get; set; }
			[Column] public int Amount { get; set; }
		}

		// A view over the PK'd table - views have no PKs in the model.
		[Table (Name = "VwOuter")]
		public class VwOuter
		{
			[Column] public int Id { get; set; }
			[Column] public string Name { get; set; }
		}

		// Composite string-key heap pair.
		[Table (Name = "StrOuter")]
		public class StrOuter
		{
			[Column] public string Code { get; set; }
			[Column] public string Region { get; set; }
		}

		[Table (Name = "StrInner")]
		public class StrInner
		{
			[Column] public string CodeRef { get; set; }
			[Column] public string RegionRef { get; set; }
			[Column] public int Val { get; set; }
		}

		#endregion

		#region Setup

		[OneTimeSetUp]
		public void SetupDatabase ()
		{
			var master = new SqlConnectionStringBuilder (TestConfig.BigJoin) { InitialCatalog = "master" }.ConnectionString;
			if (!TestConfig.CanConnect (master))
				Assert.Ignore ("Cannot connect to the SQL Server for the big-join tests. Set L2S_BIGJOIN_CONN.");

			Exec (master, "IF DB_ID('L2SBigJoinTests') IS NULL CREATE DATABASE L2SBigJoinTests");

			Exec (TestConfig.BigJoin, """
				DROP VIEW IF EXISTS VwOuter;
				DROP TABLE IF EXISTS HeapOuter; DROP TABLE IF EXISTS HeapInner;
				DROP TABLE IF EXISTS PkOuter;   DROP TABLE IF EXISTS PkInner;
				DROP TABLE IF EXISTS StrOuter;  DROP TABLE IF EXISTS StrInner;

				CREATE TABLE HeapOuter (Id INT NOT NULL, Name NVARCHAR(50) NOT NULL);
				CREATE TABLE HeapInner (IdOuter INT NOT NULL, Tipo CHAR(1) NOT NULL, Amount INT NOT NULL);
				-- The trap: cluster the inner table on a NON-key column, so an unordered join
				-- returns rows in Tipo order, interleaving the outer keys.
				CREATE CLUSTERED INDEX IX_HeapInner_Tipo ON HeapInner (Tipo, IdOuter);

				CREATE TABLE PkOuter (Id INT NOT NULL PRIMARY KEY, Name NVARCHAR(50) NOT NULL);
				CREATE TABLE PkInner (IdOuter INT NOT NULL, Tipo CHAR(1) NOT NULL, Amount INT NOT NULL,
				                      CONSTRAINT PK_PkInner PRIMARY KEY (IdOuter, Tipo));

				CREATE TABLE StrOuter (Code NVARCHAR(20) NOT NULL, Region NVARCHAR(10) NOT NULL);
				CREATE TABLE StrInner (CodeRef NVARCHAR(20) NOT NULL, RegionRef NVARCHAR(10) NOT NULL, Val INT NOT NULL);
				CREATE CLUSTERED INDEX IX_StrInner_Val ON StrInner (Val);
				""");

			Exec (TestConfig.BigJoin, "CREATE VIEW VwOuter AS SELECT Id, Name FROM PkOuter");

			Exec (TestConfig.BigJoin, """
				SET NOCOUNT ON;
				-- Ids 1..450: two inner rows each (D and N). Ids 451..475: no inner rows (empty groups).
				-- Ids 476..480: DUPLICATE outer rows (inserted twice), two inner rows each.
				WITH n AS (SELECT TOP 480 ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS i FROM sys.all_objects)
				INSERT HeapOuter SELECT i, 'Outer ' + CAST(i AS VARCHAR(10)) FROM n;
				INSERT HeapOuter SELECT Id, Name FROM HeapOuter WHERE Id BETWEEN 476 AND 480;  -- duplicates

				INSERT HeapInner SELECT Id, 'D', Id * 10     FROM HeapOuter WHERE Id <= 450 OR Id >= 476;
				INSERT HeapInner SELECT Id, 'N', Id * 10 + 1 FROM HeapOuter WHERE Id <= 450 OR Id >= 476;
				-- dedupe the inner rows produced by duplicate outers
				;WITH d AS (SELECT *, ROW_NUMBER() OVER (PARTITION BY IdOuter, Tipo ORDER BY Amount) rn FROM HeapInner)
				DELETE FROM d WHERE rn > 1;

				INSERT PkOuter SELECT DISTINCT Id, Name FROM HeapOuter;
				INSERT PkInner SELECT IdOuter, Tipo, Amount FROM HeapInner;

				-- String composite keys: 100 (code, region) pairs, 2 inner rows each, clustered so an
				-- unordered join interleaves them.
				WITH n AS (SELECT TOP 100 ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS i FROM sys.all_objects)
				INSERT StrOuter SELECT 'C' + CAST(i AS VARCHAR(10)), 'R' + CAST(i % 3 AS VARCHAR(10)) FROM n;
				INSERT StrInner SELECT Code, Region, ABS(CHECKSUM(Code)) % 1000       FROM StrOuter;
				INSERT StrInner SELECT Code, Region, ABS(CHECKSUM(Code)) % 1000 + 500 FROM StrOuter;
				""");
		}

		static void Exec (string connString, string sql)
		{
			using var cn = new SqlConnection (connString);
			cn.Open ();
			using var cmd = new SqlCommand (sql, cn) { CommandTimeout = 60 };
			cmd.ExecuteNonQuery ();
		}

		DataContext GetContext (out StringWriter log)
		{
			var ctx = new DataContext (new SqlConnection (TestConfig.BigJoin));
			log = new StringWriter ();
			ctx.Log = log;
			return ctx;
		}

		/// <summary>Number of commands the context executed, per the L2S log ("-- Context:" trailer per command).</summary>
		static int CommandCount (StringWriter log) =>
			log.ToString ().Split (new[] { "-- Context:" }, StringSplitOptions.None).Length - 1;

		#endregion

		#region Oracle helpers

		/// <summary>Fetches raw rows via plain table enumeration (no nested collections - trivially safe).</summary>
		(List<HeapOuter> outers, List<HeapInner> inners) FetchRawHeap (DataContext ctx) =>
			(ctx.GetTable<HeapOuter> ().ToList (), ctx.GetTable<HeapInner> ().ToList ());

		/// <summary>
		/// Canonicalizes a GroupJoin result to order-insensitive strings: one line per outer row
		/// (duplicates preserved), inner items sorted within the group, lines sorted overall.
		/// </summary>
		static List<string> Canonicalize<TOuter, TInner> (
			IEnumerable<(TOuter outer, IEnumerable<TInner> items)> results,
			Func<TOuter, string> outerKey, Func<TInner, string> innerKey)
			=> results
				.Select (r => outerKey (r.outer) + " => [" + string.Join (", ", r.items.Select (innerKey).OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal)
				.ToList ();

		static void AssertMatchesOracle<TOuter, TInner> (
			List<(TOuter outer, IEnumerable<TInner> items)> actual,
			List<(TOuter outer, IEnumerable<TInner> items)> expected,
			Func<TOuter, string> outerKey, Func<TInner, string> innerKey)
		{
			var actualCanon = Canonicalize (actual, outerKey, innerKey);
			var expectedCanon = Canonicalize (expected, outerKey, innerKey);
			Assert.AreEqual (expectedCanon.Count, actualCanon.Count, "group count mismatch");
			for (int i = 0; i < expectedCanon.Count; i++)
				Assert.AreEqual (expectedCanon [i], actualCanon [i], $"group mismatch at canonical index {i}");
		}

		#endregion

		#region Canary

		/// <summary>
		/// Proves the adversarial layout still has teeth: an unordered LEFT JOIN over the heap
		/// tables must return rows where the outer keys are NOT contiguous. If this ever fails,
		/// SQL Server's plan changed and the correctness tests below no longer stress ordering -
		/// the seeding strategy needs re-engineering.
		/// </summary>
		[Test]
		public void Canary_AdversarialLayoutMisordersUnorderedJoin ()
		{
			using var cn = new SqlConnection (TestConfig.BigJoin);
			cn.Open ();
			using var cmd = new SqlCommand (
				"SELECT o.Id FROM HeapOuter o LEFT JOIN HeapInner i ON o.Id = i.IdOuter", cn);
			var ids = new List<int> ();
			using (var r = cmd.ExecuteReader ())
				while (r.Read ())
					ids.Add (r.GetInt32 (0));

			int contiguousBlocks = 1;
			for (int i = 1; i < ids.Count; i++)
				if (ids [i] != ids [i - 1])
					contiguousBlocks++;

			int distinctIds = ids.Distinct ().Count ();
			// With duplicates, contiguous grouping would give at most distinct+5 blocks. Interleaving
			// gives far more (typically ~2x the row pairs). Require clear interleaving.
			Assert.Greater (contiguousBlocks, distinctIds + 100,
				"The unordered join returned near-contiguous outer keys - the adversarial layout no longer misorders, so the ordering tests have lost their teeth.");
		}

		#endregion

		#region Correctness (oracle) - must pass with any strategy

		[Test]
		public void HeapTables_GroupJoinToList_MatchesOracle ()
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o, Items = ins.ToList () })
				.ToList ()
				.Select (r => (r.o, (IEnumerable<HeapInner>)r.Items)).ToList ();

			var expected = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => o.Id.ToString (), i => $"{i.IdOuter}/{i.Tipo}/{i.Amount}");
		}

		[Test]
		public void HeapTables_GroupJoinDefaultIfEmpty_MatchesOracle ()   // xnoi's shape, incl. empty groups
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), a => a.Id, b => b.IdOuter,
					(a, b) => new { a, tab = b.DefaultIfEmpty () })
				.ToList ()
				.Select (r => (r.a, r.tab.Where (x => x != null))).ToList ();

			var expected = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => o.Id.ToString (), i => $"{i.IdOuter}/{i.Tipo}/{i.Amount}");

			// empty groups (451..475) must be present with no items
			var emptyGroups = actual.Where (r => r.a.Id >= 451 && r.a.Id <= 475).ToList ();
			Assert.AreEqual (25, emptyGroups.Count);
			Assert.IsTrue (emptyGroups.All (r => !r.Item2.Any ()), "ids 451-475 must have empty groups");
		}

		[Test]
		public void HeapTables_ProjectedInner_MatchesOracle ()   // Mythran's shape
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), ap => ap.Id, mm => mm.IdOuter,
					(ap, mms) => new { ap.Id, Groups = mms.Select (mm => new { mm.IdOuter, mm.Amount }).ToArray () })
				.ToList ();

			var expectedCanon = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o.Id, ins))
				.Select (r => r.Id + " => [" + string.Join (", ", r.ins.Select (i => $"{i.IdOuter}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();

			var actualCanon = actual
				.Select (r => r.Id + " => [" + string.Join (", ", r.Groups.Select (g => $"{g.IdOuter}/{g.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();

			CollectionAssert.AreEqual (expectedCanon, actualCanon);
		}

		[Test]
		public void HeapTables_DuplicateOuterRows_EachGetFullGroup ()
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.Where (o => o.Id >= 476)
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () })
				.ToList ();

			Assert.AreEqual (10, actual.Count, "5 duplicated ids x 2 rows each");
			foreach (var r in actual)
			{
				Assert.AreEqual (2, r.Items.Count, $"group {r.Id} must have both inner rows");
				Assert.IsTrue (r.Items.All (i => i.IdOuter == r.Id), $"group {r.Id} contains foreign rows");
				Assert.AreEqual (2, r.Items.Select (i => i.Tipo).Distinct ().Count (), $"group {r.Id} must have one D and one N row, not the same row twice");
			}
		}

		[Test]
		public void HeapTables_CompositeStringKey_MatchesOracle ()
		{
			using var ctx = GetContext (out _);
			var outers = ctx.GetTable<StrOuter> ().ToList ();
			var inners = ctx.GetTable<StrInner> ().ToList ();

			var actual = ctx.GetTable<StrOuter> ()
				.GroupJoin (ctx.GetTable<StrInner> (),
					o => new { K1 = o.Code, K2 = o.Region },
					i => new { K1 = i.CodeRef, K2 = i.RegionRef },
					(o, ins) => new { o, Items = ins.ToList () })
				.ToList ()
				.Select (r => (r.o, (IEnumerable<StrInner>)r.Items)).ToList ();

			var expected = outers
				.GroupJoin (inners,
					o => new { K1 = o.Code, K2 = o.Region },
					i => new { K1 = i.CodeRef, K2 = i.RegionRef },
					(o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => $"{o.Code}|{o.Region}", i => $"{i.CodeRef}|{i.RegionRef}|{i.Val}");
		}

		[Test]
		public void ViewOuter_GroupJoin_MatchesOracle ()
		{
			using var ctx = GetContext (out _);
			var outers = ctx.GetTable<VwOuter> ().ToList ();
			var inners = ctx.GetTable<HeapInner> ().ToList ();

			var actual = ctx.GetTable<VwOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o, Items = ins.ToList () })
				.ToList ()
				.Select (r => (r.o, (IEnumerable<HeapInner>)r.Items)).ToList ();

			var expected = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => o.Id.ToString (), i => $"{i.IdOuter}/{i.Tipo}/{i.Amount}");
		}

		[Test]
		public void HeapTables_UserOrderByPreserved_AndGroupsCorrect ()
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.OrderByDescending (o => o.Id)
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () })
				.ToList ();

			// user ordering respected
			CollectionAssert.AreEqual (
				actual.Select (r => r.Id).OrderByDescending (id => id).ToList (),
				actual.Select (r => r.Id).ToList (),
				"user OrderByDescending must be preserved");

			// groups correct
			foreach (var r in actual)
				Assert.IsTrue (r.Items.All (i => i.IdOuter == r.Id), $"group {r.Id} contains foreign rows");

			Assert.AreEqual (outers.Count, actual.Count);
		}

		[Test]
		public void MixedJoinOuter_PkJoinedToHeap_MatchesOracle ()
		{
			using var ctx = GetContext (out _);
			var (heapOuters, inners) = FetchRawHeap (ctx);
			var pkOuters = ctx.GetTable<PkOuter> ().ToList ();

			var actual = ctx.GetTable<PkOuter> ()
				.Join (ctx.GetTable<HeapOuter> (), p => p.Id, h => h.Id, (p, h) => new { p, h })
				.GroupJoin (ctx.GetTable<HeapInner> (), x => x.h.Id, i => i.IdOuter,
					(x, ins) => new { x.h.Id, Items = ins.ToList () })
				.ToList ();

			var expected = pkOuters
				.Join (heapOuters, p => p.Id, h => h.Id, (p, h) => new { p, h })
				.GroupJoin (inners, x => x.h.Id, i => i.IdOuter, (x, ins) => new { x.h.Id, ins })
				.ToList ();

			var actualCanon = actual
				.Select (r => r.Id + " => [" + string.Join (", ", r.Items.Select (i => $"{i.IdOuter}/{i.Tipo}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();
			var expectedCanon = expected
				.Select (r => r.Id + " => [" + string.Join (", ", r.ins.Select (i => $"{i.IdOuter}/{i.Tipo}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();

			CollectionAssert.AreEqual (expectedCanon, actualCanon);
		}

		[Test]
		public void NestedWhereCollectionProjection_HeapTables_MatchesOracle ()   // multiset not via GroupJoin
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);
			var innerTable = ctx.GetTable<HeapInner> ();

			var actual = ctx.GetTable<HeapOuter> ()
				.Select (o => new { o.Id, Items = innerTable.Where (i => i.IdOuter == o.Id).ToList () })
				.ToList ();

			var expectedCanon = outers
				.Select (o => o.Id + " => [" + string.Join (", ", inners.Where (i => i.IdOuter == o.Id).Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();
			var actualCanon = actual
				.Select (r => r.Id + " => [" + string.Join (", ", r.Items.Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();

			CollectionAssert.AreEqual (expectedCanon, actualCanon);
		}

		[Test]
		public void TwoMultisetsInProjection_HeapTables_MatchesOracle ()
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);
			var innerTable = ctx.GetTable<HeapInner> ();

			var actual = ctx.GetTable<HeapOuter> ()
				.Select (o => new
				{
					o.Id,
					All = innerTable.Where (i => i.IdOuter == o.Id).ToList (),
					Ds = innerTable.Where (i => i.IdOuter == o.Id && i.Tipo == "D").ToList (),
				})
				.ToList ();

			foreach (var r in actual)
			{
				var expAll = inners.Where (i => i.IdOuter == r.Id).Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal);
				var expDs = inners.Where (i => i.IdOuter == r.Id && i.Tipo == "D").Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal);
				CollectionAssert.AreEqual (expAll.ToList (), r.All.Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal).ToList (), $"'All' mismatch for {r.Id}");
				CollectionAssert.AreEqual (expDs.ToList (), r.Ds.Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal).ToList (), $"'Ds' mismatch for {r.Id}");
			}
		}

		[Test]
		public void DistinctOuter_GroupJoin_MatchesOracle ()   // Distinct forbids big join entirely
		{
			using var ctx = GetContext (out _);
			var (outers, inners) = FetchRawHeap (ctx);

			var actual = ctx.GetTable<HeapOuter> ()
				.Select (o => o.Id).Distinct ()
				.GroupJoin (ctx.GetTable<HeapInner> (), id => id, i => i.IdOuter,
					(id, ins) => new { Id = id, Items = ins.ToList () })
				.ToList ();

			var expectedCanon = outers.Select (o => o.Id).Distinct ()
				.GroupJoin (inners, id => id, i => i.IdOuter, (id, ins) => (id, ins))
				.Select (r => r.id + " => [" + string.Join (", ", r.ins.Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();
			var actualCanon = actual
				.Select (r => r.Id + " => [" + string.Join (", ", r.Items.Select (i => $"{i.Tipo}/{i.Amount}").OrderBy (s => s, StringComparer.Ordinal)) + "]")
				.OrderBy (s => s, StringComparer.Ordinal).ToList ();

			CollectionAssert.AreEqual (expectedCanon, actualCanon);
		}

		[Test]
		public void PkTables_GroupJoin_MatchesOracle ()   // control: mainstream path
		{
			using var ctx = GetContext (out _);
			var outers = ctx.GetTable<PkOuter> ().ToList ();
			var inners = ctx.GetTable<PkInner> ().ToList ();

			var actual = ctx.GetTable<PkOuter> ()
				.GroupJoin (ctx.GetTable<PkInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o, Items = ins.ToList () })
				.ToList ()
				.Select (r => (r.o, (IEnumerable<PkInner>)r.Items)).ToList ();

			var expected = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => o.Id.ToString (), i => $"{i.IdOuter}/{i.Tipo}/{i.Amount}");
		}

		[Test]
		public void PkOuterHeapInner_GroupJoin_MatchesOracle ()   // outer PK suffices for contiguity
		{
			using var ctx = GetContext (out _);
			var outers = ctx.GetTable<PkOuter> ().ToList ();
			var inners = ctx.GetTable<HeapInnerForPkOuter> ().ToList ();

			var actual = ctx.GetTable<PkOuter> ()
				.GroupJoin (ctx.GetTable<HeapInnerForPkOuter> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o, Items = ins.ToList () })
				.ToList ()
				.Select (r => (r.o, (IEnumerable<HeapInnerForPkOuter>)r.Items)).ToList ();

			var expected = outers
				.GroupJoin (inners, o => o.Id, i => i.IdOuter, (o, ins) => (o, ins))
				.ToList ();

			AssertMatchesOracle (actual, expected, o => o.Id.ToString (), i => $"{i.IdOuter}/{i.Tipo}/{i.Amount}");
		}

		#endregion

		#region Strategy - pins WHICH execution strategy runs

		[Test]
		public void Strategy_HeapTables_SingleQueryWithSynthesizedOrdering ()
		{
			using var ctx = GetContext (out var log);

			var q = ctx.GetTable<HeapOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () });

			string sql = ctx.GetCommand (q).CommandText;
			StringAssert.Contains ("ROW_NUMBER", sql,
				"PK-less big join must synthesize a ROW_NUMBER ordering over the outer rows");
			StringAssert.Contains ("ORDER BY", sql, "PK-less big join must emit an ORDER BY");

			var results = q.ToList ();
			Assert.AreEqual (485, results.Count);
			Assert.AreEqual (1, CommandCount (log),
				"PK-less GroupJoin should run as ONE big-join query, not one query per outer row");
		}

		[Test]
		public void Strategy_ViewOuter_SingleQueryWithSynthesizedOrdering ()
		{
			using var ctx = GetContext (out var log);

			var q = ctx.GetTable<VwOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () });

			string sql = ctx.GetCommand (q).CommandText;
			StringAssert.Contains ("ROW_NUMBER", sql);
			StringAssert.Contains ("ORDER BY", sql);

			q.ToList ();
			Assert.AreEqual (1, CommandCount (log));
		}

		[Test]
		public void Strategy_PkTables_UnchangedBigJoin_NoRowNumber ()
		{
			using var ctx = GetContext (out var log);

			var q = ctx.GetTable<PkOuter> ()
				.GroupJoin (ctx.GetTable<PkInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () });

			string sql = ctx.GetCommand (q).CommandText;
			StringAssert.DoesNotContain ("ROW_NUMBER", sql,
				"PK'd sources must keep the plain PK ordering - no synthesized row number");
			StringAssert.Contains ("ORDER BY [t0].[Id]", sql, "PK'd big join must order by the outer PK");

			q.ToList ();
			Assert.AreEqual (1, CommandCount (log), "PK'd GroupJoin must stay a single big-join query");
		}

		[Test]
		public void Strategy_DistinctOuter_StillFallsBackToPerRowQueries ()
		{
			using var ctx = GetContext (out var log);

			var q = ctx.GetTable<HeapOuter> ()
				.Select (o => o.Id).Distinct ()
				.GroupJoin (ctx.GetTable<HeapInner> (), id => id, i => i.IdOuter,
					(id, ins) => new { Id = id, Items = ins.ToList () });

			q.ToList ();
			Assert.Greater (CommandCount (log), 1,
				"Distinct outer cannot big-join (PK lifting is impossible); it must keep the per-row fallback");
		}

		[Test]
		public void Strategy_Sql2000Mode_NoRowNumber ()
		{
			using var ctx = GetContext (out _);
			ctx.PerInstanceProviderMode = SqlServerProviderMode.Sql2000;

			var q = ctx.GetTable<HeapOuter> ()
				.GroupJoin (ctx.GetTable<HeapInner> (), o => o.Id, i => i.IdOuter,
					(o, ins) => new { o.Id, Items = ins.ToList () });

			string sql = ctx.GetCommand (q).CommandText;
			StringAssert.DoesNotContain ("ROW_NUMBER", sql, "SQL2000 has no ROW_NUMBER support");

			var results = q.ToList ();
			Assert.AreEqual (485, results.Count);
			foreach (var r in results)
				Assert.IsTrue (r.Items.All (i => i.IdOuter == r.Id), $"group {r.Id} contains foreign rows in Sql2000 mode");
		}

		#endregion
	}
}

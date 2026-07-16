using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ReadWriteTests.SqlServer
{
	/// <summary>
	/// Tests for the set-based ExecuteUpdate / ExecuteDelete operators. The fixture creates its own
	/// database (L2SExecuteDmlTests) and reseeds the Widget table before every test.
	/// </summary>
	[TestFixture]
	public class ExecuteDmlTests
	{
		[Table (Name = "Widget")]
		public class Widget
		{
			[Column (IsPrimaryKey = true)] public int Id { get; set; }
			[Column] public string Name { get; set; }
			[Column] public int Stock { get; set; }
			[Column] public decimal Price { get; set; }
			[Column (CanBeNull = true)] public string Category { get; set; }
		}

		[OneTimeSetUp]
		public void SetupDatabase ()
		{
			var master = new SqlConnectionStringBuilder (TestConfig.ExecuteDml) { InitialCatalog = "master" }.ConnectionString;
			if (!TestConfig.CanConnect (master))
				Assert.Ignore ("Cannot connect to the SQL Server for the ExecuteDml tests. Set L2S_EXECUTEDML_CONN.");

			Exec (master, "IF DB_ID('L2SExecuteDmlTests') IS NULL CREATE DATABASE L2SExecuteDmlTests");
			Exec (TestConfig.ExecuteDml, """
				DROP TABLE IF EXISTS Widget;
				CREATE TABLE Widget (
					Id INT NOT NULL PRIMARY KEY,
					Name NVARCHAR(50) NOT NULL,
					Stock INT NOT NULL,
					Price DECIMAL(9,2) NOT NULL,
					Category NVARCHAR(20) NULL);
				""");
		}

		[SetUp]
		public void SeedTable ()
		{
			Exec (TestConfig.ExecuteDml, """
				DELETE FROM Widget;
				INSERT INTO Widget (Id, Name, Stock, Price, Category) VALUES
					(1, 'Anvil',   10, 100.00, 'Heavy'),
					(2, 'Hammer',   5,  25.50, 'Heavy'),
					(3, 'Feather',  0,   1.00, 'Light'),
					(4, 'Balloon',  0,   2.50, 'Light'),
					(5, 'Widget',  99,  10.00, NULL),
					(6, 'Gadget',  42,  50.00, NULL);
				""");
		}


		[Test]
		public void ExecuteDeleteRemovesMatchingRows ()
		{
			using var ctx = GetContext (out var log);

			int deleted = ctx.GetTable<Widget> ().Where (w => w.Stock == 0).ExecuteDelete ();

			Assert.AreEqual (2, deleted);
			StringAssert.Contains ("DELETE FROM", log.ToString ());

			using var check = GetContext (out _);
			CollectionAssert.AreEquivalent (new[] { 1, 2, 5, 6 }, check.GetTable<Widget> ().Select (w => w.Id).ToList ());
		}


		[Test]
		public void ExecuteDeleteWithoutPredicateRemovesAllRows ()
		{
			using var ctx = GetContext (out _);

			int deleted = ctx.GetTable<Widget> ().ExecuteDelete ();

			Assert.AreEqual (6, deleted);
			using var check = GetContext (out _);
			Assert.AreEqual (0, check.GetTable<Widget> ().Count ());
		}


		[Test]
		public void ExecuteDeleteWithChainedWheres ()
		{
			using var ctx = GetContext (out _);

			int deleted = ctx.GetTable<Widget> ().Where (w => w.Stock == 0).Where (w => w.Price > 2).ExecuteDelete ();

			Assert.AreEqual (1, deleted);
			using var check = GetContext (out _);
			CollectionAssert.AreEquivalent (new[] { 1, 2, 3, 5, 6 }, check.GetTable<Widget> ().Select (w => w.Id).ToList ());
		}


		[Test]
		public void ExecuteDeleteReturnsZeroWhenNothingMatches ()
		{
			using var ctx = GetContext (out _);

			Assert.AreEqual (0, ctx.GetTable<Widget> ().Where (w => w.Id > 1000).ExecuteDelete ());
			using var check = GetContext (out _);
			Assert.AreEqual (6, check.GetTable<Widget> ().Count ());
		}


		[Test]
		public void ExecuteUpdateWithConstantValue ()
		{
			using var ctx = GetContext (out var log);

			int updated = ctx.GetTable<Widget> ().Where (w => w.Category == null)
							 .ExecuteUpdate (s => s.SetProperty (w => w.Category, "Misc"));

			Assert.AreEqual (2, updated);
			StringAssert.Contains ("UPDATE", log.ToString ());

			using var check = GetContext (out _);
			Assert.AreEqual (0, check.GetTable<Widget> ().Count (w => w.Category == null));
			Assert.AreEqual (2, check.GetTable<Widget> ().Count (w => w.Category == "Misc"));
		}


		[Test]
		public void ExecuteUpdateWithComputedValuesAndMultipleSetters ()
		{
			using var ctx = GetContext (out _);

			int updated = ctx.GetTable<Widget> ().Where (w => w.Category == "Heavy")
							 .ExecuteUpdate (s => s
								 .SetProperty (w => w.Stock, w => w.Stock + 10)
								 .SetProperty (w => w.Price, w => w.Price * 2)
								 .SetProperty (w => w.Name, w => w.Name + "!"));

			Assert.AreEqual (2, updated);

			using var check = GetContext (out _);
			var anvil = check.GetTable<Widget> ().Single (w => w.Id == 1);
			Assert.AreEqual ("Anvil!", anvil.Name);
			Assert.AreEqual (20, anvil.Stock);
			Assert.AreEqual (200.00m, anvil.Price);
			var hammer = check.GetTable<Widget> ().Single (w => w.Id == 2);
			Assert.AreEqual ("Hammer!", hammer.Name);
			Assert.AreEqual (15, hammer.Stock);
			Assert.AreEqual (51.00m, hammer.Price);
			// unmatched rows must be untouched
			Assert.AreEqual ("Feather", check.GetTable<Widget> ().Single (w => w.Id == 3).Name);
		}


		[Test]
		public void ExecuteUpdateCanSetNull ()
		{
			using var ctx = GetContext (out _);

			int updated = ctx.GetTable<Widget> ().Where (w => w.Category == "Light")
							 .ExecuteUpdate (s => s.SetProperty (w => w.Category, (string)null));

			Assert.AreEqual (2, updated);
			using var check = GetContext (out _);
			Assert.AreEqual (4, check.GetTable<Widget> ().Count (w => w.Category == null));
		}


		[Test]
		public void ExecuteUpdateWithCapturedValue ()
		{
			using var ctx = GetContext (out _);
			decimal surcharge = 0.75m;

			int updated = ctx.GetTable<Widget> ().Where (w => w.Id == 5)
							 .ExecuteUpdate (s => s.SetProperty (w => w.Price, w => w.Price + surcharge));

			Assert.AreEqual (1, updated);
			using var check = GetContext (out _);
			Assert.AreEqual (10.75m, check.GetTable<Widget> ().Single (w => w.Id == 5).Price);
		}


		[Test]
		public void ExecuteDeleteWithContainsPredicate ()
		{
			using var ctx = GetContext (out _);
			var ids = new[] { 2, 4, 6 };

			int deleted = ctx.GetTable<Widget> ().Where (w => ids.Contains (w.Id)).ExecuteDelete ();

			Assert.AreEqual (3, deleted);
			using var check = GetContext (out _);
			CollectionAssert.AreEquivalent (new[] { 1, 3, 5 }, check.GetTable<Widget> ().Select (w => w.Id).ToList ());
		}


		[Test]
		public void ExecuteDeleteOnProjectionThrows ()
		{
			using var ctx = GetContext (out _);

			Assert.Throws<NotSupportedException> (() => ctx.GetTable<Widget> ().Select (w => w.Name).ExecuteDelete ());
		}


		[Test]
		public void ExecuteDeleteAfterTakeThrows ()
		{
			using var ctx = GetContext (out _);

			Assert.Throws<NotSupportedException> (() => ctx.GetTable<Widget> ().OrderBy (w => w.Id).Take (3).ExecuteDelete ());
		}


		[Test]
		public void ExecuteUpdateWithNonSetterLambdaThrows ()
		{
			using var ctx = GetContext (out _);

			Assert.Throws<NotSupportedException> (() =>
				ctx.GetTable<Widget> ().ExecuteUpdate (s => s));
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
			log = new StringWriter ();
			return new DataContext (new SqlConnection (TestConfig.ExecuteDml)) { Log = log };
		}
	}
}

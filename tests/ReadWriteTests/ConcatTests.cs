using System;
using System.Linq;
using NUnit.Framework;

using System.Data.Linq;
using ReadTestsAdventureWorks2008;
using System.Data.Linq.Mapping;

using System.Data.SqlClient;

namespace ReadWriteTests.SqlServer
{
	/// <summary>
	/// Tests for Concat/Union queries with more than two branches.
	/// </summary>
	[TestFixture]
	public class ConcatTests
	{
		#region Members
		private MappingSource _mappingSourceFromXmlFile;
		#endregion


		[OneTimeSetUp]
		public void SetupTests()
		{
			if(!TestConfig.CanConnect(TestConfig.AdventureWorks))
			{
				Assert.Ignore("Cannot connect to the AdventureWorks test database. " +
					"Set L2S_ADVENTUREWORKS_CONN to a SQL Server hosting the AdventureWorks 2008R2+ sample.");
			}
		}


		[Test]
		public void TripleConcatAlignsAllBranches()
		{
			using(var ctx = GetContext())
			{
				// regression test for ExpressionDuplicator.ExpandTogether's SqlNodeType.New case, which read
				// branch 2's object construction where it should read branch i's: with three or more union
				// branches, the third branch's member expressions never participated in the union's column
				// alignment. Each branch here computes the same member differently, so misalignment shows up
				// as wrong data rather than luck-of-the-ordering success.
				var query = ctx.Products.Select(p => new { Id = p.ProductId, Src = 1 })
								.Concat(ctx.Products.Select(p => new { Id = p.ProductId + 100000, Src = 2 }))
								.Concat(ctx.Products.Select(p => new { Id = p.ProductId + 200000, Src = 3 }));

				var actual = query.ToList();

				var ids = ctx.Products.Select(p => p.ProductId).ToList();
				var expected = ids.Select(id => new { Id = id, Src = 1 })
								.Concat(ids.Select(id => new { Id = id + 100000, Src = 2 }))
								.Concat(ids.Select(id => new { Id = id + 200000, Src = 3 }))
								.ToList();

				CollectionAssert.AreEqual(
					expected.OrderBy(x => x.Id).ToList(),
					actual.OrderBy(x => x.Id).ToList());
			}
		}


		[Test]
		public void TripleConcatOfEntities()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.ProductSubcategories
								.Concat(ctx.ProductSubcategories)
								.Concat(ctx.ProductSubcategories);

				var actual = query.ToList();

				Assert.AreEqual(ctx.ProductSubcategories.Count() * 3, actual.Count);
				foreach(var group in actual.GroupBy(sc => sc.ProductSubcategoryId))
				{
					Assert.AreEqual(3, group.Count(), "Each subcategory must appear exactly once per branch");
					Assert.AreEqual(1, group.Select(sc => sc.Name).Distinct().Count(), "All copies must materialize identically");
				}
			}
		}


		private AdventureWorks2008DataContext GetContext()
		{
			if(_mappingSourceFromXmlFile == null)
			{
				var modelAssembly = typeof(AdventureWorks2008DataContext).Assembly;
				var resourceStream = modelAssembly.GetManifestResourceStream("AdventureWorks2008.AdventureWorks2008Mappings.xml");
				_mappingSourceFromXmlFile = XmlMappingSource.FromStream(resourceStream);
			}
			var connection = new SqlConnection(TestConfig.AdventureWorks);
			return new AdventureWorks2008DataContext(connection, _mappingSourceFromXmlFile);
		}
	}
}

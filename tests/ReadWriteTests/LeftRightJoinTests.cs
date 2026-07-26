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
	/// Tests for the LeftJoin / RightJoin sequence operators introduced in .NET 10.
	/// </summary>
	[TestFixture]
	public class LeftRightJoinTests
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
		public void LeftJoinMatchesGroupJoinDefaultIfEmptyPattern()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products
								.LeftJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId, OrderQty = (short?)d.OrderQty })
								.ToList();

				var expected = (from p in ctx.Products
								join d in ctx.SalesOrderDetails on p.ProductId equals d.ProductId into g
								from d in g.DefaultIfEmpty()
								select new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId, OrderQty = (short?)d.OrderQty })
								.ToList();

				// unordered comparison: sort both result sets the same way.
				actual = actual.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();
				expected = expected.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);

				// AdventureWorks contains products which were never ordered, so both matched and unmatched rows must be present.
				Assert.IsTrue(actual.Any(x => x.DetailId == null), "Expected unmatched outer rows with a null inner side");
				Assert.IsTrue(actual.Any(x => x.DetailId != null), "Expected matched rows");
			}
		}


		[Test]
		public void RightJoinMatchesLeftJoinWithSidesSwapped()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.SalesOrderDetails
								.RightJoin(ctx.Products,
									d => d.ProductId,
									p => p.ProductId,
									(d, p) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.ToList();

				var expected = ctx.Products
								.LeftJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.ToList();

				actual = actual.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();
				expected = expected.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
				Assert.IsTrue(actual.Any(x => x.DetailId == null), "Expected unmatched preserved rows with a null outer side");
			}
		}


		[Test]
		public void LeftJoinProjectingInnerEntity()
		{
			using(var ctx = GetContext())
			{
				var results = ctx.Products
								.LeftJoin(ctx.ProductSubcategories,
									p => p.ProductSubcategoryId,
									sc => (int?)sc.ProductSubcategoryId,
									(p, sc) => new { Product = p, Subcategory = sc })
								.ToList();

				// each product has at most one subcategory, so no row multiplication may occur.
				Assert.AreEqual(ctx.Products.Count(), results.Count);
				Assert.IsTrue(results.Any(x => x.Subcategory == null), "Expected products without a subcategory to have a null inner entity");
				foreach(var row in results)
				{
					if(row.Product.ProductSubcategoryId == null)
					{
						Assert.IsNull(row.Subcategory);
					}
					else
					{
						Assert.IsNotNull(row.Subcategory);
						Assert.AreEqual(row.Product.ProductSubcategoryId, row.Subcategory.ProductSubcategoryId);
					}
				}
			}
		}


		[Test]
		public void LeftJoinWithCompositeKey()
		{
			using(var ctx = GetContext())
			{
				var detailsQuery = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 43700);

				var results = detailsQuery
								.LeftJoin(ctx.SpecialOfferProducts,
									d => new { d.SpecialOfferId, d.ProductId },
									so => new { so.SpecialOfferId, so.ProductId },
									(d, so) => new { d.SalesOrderDetailId, SpecialOfferModifiedDate = (DateTime?)so.ModifiedDate })
								.ToList();

				// every sales order detail references an existing SpecialOfferProduct row, so all rows must match.
				Assert.AreEqual(detailsQuery.Count(), results.Count);
				CollectionAssert.IsNotEmpty(results);
				Assert.IsTrue(results.All(x => x.SpecialOfferModifiedDate != null), "Expected every row to have a match on the composite key");
			}
		}


		[Test]
		public void LeftJoinFollowedByWhereOnInnerNull()
		{
			using(var ctx = GetContext())
			{
				// products which were never ordered, filtered server-side on the null-extended inner side.
				var actual = ctx.Products
								.LeftJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.Where(x => x.DetailId == null)
								.Count();

				var expected = (from p in ctx.Products
								join d in ctx.SalesOrderDetails on p.ProductId equals d.ProductId into g
								from d in g.DefaultIfEmpty()
								select new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.Where(x => x.DetailId == null)
								.Count();

				Assert.Greater(actual, 0);
				Assert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void LeftJoinGeneratesLeftOuterJoinSql()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products
								.LeftJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId });

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("LEFT OUTER JOIN", commandText, "Expected the OUTER APPLY construct to be reduced to a LEFT OUTER JOIN");
			}
		}


		[Test]
		public void LeftJoinOverGroupedSourcesGeneratesLeftOuterJoinSql()
		{
			using(var ctx = GetContext())
			{
				// grouped/aggregated sources cannot be reduced from the APPLY construct by SqlOuterApplyReducer,
				// so this verifies that the join is built directly as a LEFT OUTER JOIN.
				var salesA = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 44000)
								.GroupBy(d => d.ProductId)
								.Select(g => new { ProductID = (int?)g.Key, Revenue = (decimal?)g.Sum(x => x.LineTotal) });
				var salesB = ctx.SalesOrderDetails.Where(d => d.SalesOrderId >= 44000)
								.GroupBy(d => d.ProductId)
								.Select(g => new { ProductID = (int?)g.Key, Revenue = (decimal?)g.Sum(x => x.LineTotal) });

				var query = salesA.LeftJoin(salesB,
								a => a.ProductID,
								b => b.ProductID,
								(a, b) => new { a.ProductID, RevA = a.Revenue, RevB = b.Revenue });

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("LEFT OUTER JOIN", commandText, "Expected a directly constructed LEFT OUTER JOIN");
				StringAssert.DoesNotContain("APPLY", commandText, "Grouped sources must be joined directly rather than via APPLY");

				var actual = query.ToList()
								.OrderBy(x => x.ProductID).ThenBy(x => x.RevB).ToList();

				var listA = salesA.ToList();
				var listB = salesB.ToList();
				var expected = listA.LeftJoin(listB,
								a => a.ProductID,
								b => b.ProductID,
								(a, b) => new { a.ProductID, RevA = a.Revenue, RevB = b == null ? (decimal?)null : b.Revenue })
								.OrderBy(x => x.ProductID).ThenBy(x => x.RevB).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void RightJoinGeneratesLeftOuterJoinSql()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.SalesOrderDetails
								.RightJoin(ctx.Products,
									d => d.ProductId,
									p => p.ProductId,
									(d, p) => new { p.ProductId, DetailId = (int?)d.SalesOrderDetailId });

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("LEFT OUTER JOIN", commandText, "Expected the OUTER APPLY construct to be reduced to a LEFT OUTER JOIN");
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

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using System.Data.Linq;
using ReadTestsAdventureWorks2008;
using System.Data.Linq.Mapping;

using System.Data.SqlClient;

namespace ReadWriteTests.SqlServer
{
	/// <summary>
	/// Tests for the FullJoin sequence operator introduced in .NET 11, translated as a native FULL OUTER JOIN.
	/// </summary>
	[TestFixture]
	public class FullJoinTests
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
		public void FullJoinProducesMatchedAndBothSidedUnmatchedRows()
		{
			using(var ctx = GetContext())
			{
				// self-join on a shifted key guarantees unmatched rows on both sides
				// (the lowest ProductId has no left match, the highest no right match).
				var actual = ctx.Products
								.FullJoin(ctx.Products,
									p => p.ProductId,
									q => q.ProductId + 1,
									(p, q) => new { A = (int?)p.ProductId, B = (int?)q.ProductId })
								.ToList();

				var ids = ctx.Products.Select(p => (int?)p.ProductId).ToList();
				var expected = ids.FullJoin(ids,
									a => a,
									b => b + 1,
									(a, b) => new { A = a, B = b })
								.ToList();

				actual = actual.OrderBy(x => x.A).ThenBy(x => x.B).ToList();
				expected = expected.OrderBy(x => x.A).ThenBy(x => x.B).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
				Assert.IsTrue(actual.Any(x => x.A == null), "Expected unmatched rows preserved from the inner side");
				Assert.IsTrue(actual.Any(x => x.B == null), "Expected unmatched rows preserved from the outer side");
				Assert.IsTrue(actual.Any(x => x.A != null && x.B != null), "Expected matched rows");
			}
		}


		[Test]
		public void FullJoinEquivalentToLeftJoinWhenEveryInnerRowMatches()
		{
			using(var ctx = GetContext())
			{
				// every sales order detail references an existing product, so a full join
				// must produce exactly the left join's rows.
				var actual = ctx.Products
								.FullJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { ProductId = (int?)p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.ToList();

				var expected = ctx.Products
								.LeftJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { ProductId = (int?)p.ProductId, DetailId = (int?)d.SalesOrderDetailId })
								.ToList();

				actual = actual.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();
				expected = expected.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void FullJoinProjectingEntities()
		{
			using(var ctx = GetContext())
			{
				var results = ctx.Products
								.FullJoin(ctx.Products,
									p => p.ProductId,
									q => q.ProductId + 1,
									(p, q) => new { P = p, Q = q })
								.ToList();

				var products = ctx.Products.ToList();
				var expectedCount = products.FullJoin(products, p => p.ProductId, q => q.ProductId + 1, (p, q) => 1).Count();

				Assert.AreEqual(expectedCount, results.Count);
				Assert.IsTrue(results.Any(x => x.P == null), "Expected null entities on the outer side");
				Assert.IsTrue(results.Any(x => x.Q == null), "Expected null entities on the inner side");
				foreach(var row in results)
				{
					Assert.IsTrue(row.P != null || row.Q != null, "A full join row cannot have both sides null");
					if(row.P != null && row.Q != null)
					{
						Assert.AreEqual(row.P.ProductId, row.Q.ProductId + 1);
					}
				}
			}
		}


		[Test]
		public void FullJoinGeneratesFullOuterJoinSql()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products
								.FullJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { ProductId = (int?)p.ProductId, DetailId = (int?)d.SalesOrderDetailId });

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("FULL OUTER JOIN", commandText, "Expected the union emulation to be reduced to a native FULL OUTER JOIN");
				StringAssert.DoesNotContain("UNION", commandText, "Expected the union emulation to have been collapsed entirely");
			}
		}


		[Test]
		public void FullJoinOverGroupedSourcesGeneratesFullOuterJoinSql()
		{
			using(var ctx = GetContext())
			{
				// the year-over-year reporting shape: both sides are grouped/aggregated subqueries.
				var salesA = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 44000)
								.GroupBy(d => d.ProductId)
								.Select(g => new { ProductID = (int?)g.Key, Revenue = (decimal?)g.Sum(x => x.LineTotal) });
				var salesB = ctx.SalesOrderDetails.Where(d => d.SalesOrderId >= 44000)
								.GroupBy(d => d.ProductId)
								.Select(g => new { ProductID = (int?)g.Key, Revenue = (decimal?)g.Sum(x => x.LineTotal) });

				var query = salesA.FullJoin(salesB,
								a => a.ProductID,
								b => b.ProductID,
								(a, b) => new { Id = a.ProductID ?? b.ProductID, RevA = a.Revenue, RevB = b.Revenue });

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("FULL OUTER JOIN", commandText, "Expected the union emulation to be reduced to a native FULL OUTER JOIN");
				StringAssert.DoesNotContain("UNION", commandText, "Expected the union emulation to have been collapsed entirely");
				StringAssert.DoesNotContain("APPLY", commandText, "Grouped sources must be joined directly rather than via APPLY");

				var actual = query.ToList()
								.OrderBy(x => x.Id).ThenBy(x => x.RevA).ToList();

				var listA = salesA.ToList();
				var listB = salesB.ToList();
				var expected = listA.FullJoin(listB,
								a => a.ProductID,
								b => b.ProductID,
								(a, b) => new
								{
									Id = (a == null ? null : a.ProductID) ?? (b == null ? null : b.ProductID),
									RevA = a == null ? (decimal?)null : a.Revenue,
									RevB = b == null ? (decimal?)null : b.Revenue
								})
								.OrderBy(x => x.Id).ThenBy(x => x.RevA).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void FullJoinOnFilteredSourcesRemainsCorrect()
		{
			using(var ctx = GetContext())
			{
				// a filter on a join source must stay inside the join operand - hoisting it above the full
				// join would wrongly discard null-extended rows (WhereClauseLifter blocks hoisting out of
				// either side of a FULL OUTER JOIN).
				var outerQuery = ctx.Products.Where(p => p.ProductId >= 900);
				var innerQuery = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 43700);

				var actualQuery = outerQuery
								.FullJoin(innerQuery,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { ProductId = (int?)p.ProductId, DetailId = (int?)d.SalesOrderDetailId });

				string commandText = ctx.GetCommand(actualQuery).CommandText;
				StringAssert.Contains("FULL OUTER JOIN", commandText);
				StringAssert.DoesNotContain("UNION", commandText);

				var actual = actualQuery.ToList();

				var outerIds = outerQuery.Select(p => (int?)p.ProductId).ToList();
				var innerRows = innerQuery.Select(d => new { d.SalesOrderDetailId, d.ProductId }).ToList();
				var expected = outerIds.FullJoin(innerRows,
									a => a,
									d => (int?)d.ProductId,
									(a, d) => new { ProductId = a, DetailId = d == null ? (int?)null : d.SalesOrderDetailId })
								.ToList();

				actual = actual.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();
				expected = expected.OrderBy(x => x.ProductId).ThenBy(x => x.DetailId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
				Assert.IsTrue(actual.Any(x => x.ProductId == null), "Expected right-only rows (details of products below 900)");
			}
		}


		[Test]
		public void FullJoinFollowedByWhereOnNullSide()
		{
			using(var ctx = GetContext())
			{
				// filtering on the null-extended outer side after a full join yields the anti-join rows.
				var actual = ctx.Products
								.FullJoin(ctx.Products,
									p => p.ProductId,
									q => q.ProductId + 1,
									(p, q) => new { A = (int?)p.ProductId, B = (int?)q.ProductId })
								.Where(x => x.A == null)
								.Count();

				var ids = ctx.Products.Select(p => (int?)p.ProductId).ToList();
				var expected = ids.FullJoin(ids, a => a, b => b + 1, (a, b) => new { A = a, B = b })
								.Count(x => x.A == null);

				Assert.Greater(actual, 0);
				Assert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void FullJoinWithCompositeKey()
		{
			using(var ctx = GetContext())
			{
				var detailsQuery = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 43700);

				var actual = detailsQuery
								.FullJoin(ctx.SpecialOfferProducts,
									d => new { d.SpecialOfferId, d.ProductId },
									so => new { so.SpecialOfferId, so.ProductId },
									(d, so) => new { DetailId = (int?)d.SalesOrderDetailId, OfferProductId = (int?)so.ProductId })
								.ToList();

				var details = detailsQuery.Select(d => new { d.SalesOrderDetailId, d.SpecialOfferId, d.ProductId }).ToList();
				var offers = ctx.SpecialOfferProducts.Select(so => new { so.SpecialOfferId, so.ProductId }).ToList();
				var expected = details.FullJoin(offers,
									d => new { d.SpecialOfferId, d.ProductId },
									so => new { so.SpecialOfferId, so.ProductId },
									(d, so) => new
									{
										DetailId = d == null ? (int?)null : d.SalesOrderDetailId,
										OfferProductId = so == null ? (int?)null : so.ProductId
									})
								.ToList();

				actual = actual.OrderBy(x => x.DetailId).ThenBy(x => x.OfferProductId).ToList();
				expected = expected.OrderBy(x => x.DetailId).ThenBy(x => x.OfferProductId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
				Assert.IsTrue(actual.Any(x => x.DetailId == null), "Expected unreferenced special offer products as right-only rows");
			}
		}


		[Test]
		public void FullJoinWithNavigationsInResultSelector()
		{
			using(var ctx = GetContext())
			{
				// association navigations in the result selector stack extra joins on top of the full join.
				// Both sides of a FULL OUTER JOIN are null-extended, so the binder's outer-dependent handling
				// must plan every such navigation as a LEFT OUTER JOIN - so.Product's non-nullable FK would
				// otherwise yield an INNER join, filtering out null-extended rows with no navigation match.
				var detailsQuery = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 43700);

				var query = ctx.SpecialOfferProducts
								.FullJoin(detailsQuery,
									so => new { so.SpecialOfferId, so.ProductId },
									d => new { d.SpecialOfferId, d.ProductId },
									(so, d) => new
									{
										OfferProductId = (int?)so.ProductId,
										ProductName = so.Product.Name,
										DetailId = (int?)d.SalesOrderDetailId,
										OrderDate = (DateTime?)d.SalesOrderHeader.OrderDate
									});

				string commandText = ctx.GetCommand(query).CommandText;
				StringAssert.Contains("FULL OUTER JOIN", commandText, "Expected the union emulation to be reduced to a native FULL OUTER JOIN");
				StringAssert.DoesNotContain("UNION", commandText, "Expected the union emulation to have been collapsed entirely");
				StringAssert.DoesNotContain("INNER JOIN", commandText, "Expected navigation joins to be demoted to LEFT OUTER JOIN, since either join side can be null-extended");

				var actual = query.ToList()
								.OrderBy(x => x.OfferProductId).ThenBy(x => x.DetailId).ToList();

				var offers = ctx.SpecialOfferProducts
								.Select(so => new { so.SpecialOfferId, so.ProductId, so.Product.Name }).ToList();
				var details = detailsQuery
								.Select(d => new { d.SalesOrderDetailId, d.SpecialOfferId, d.ProductId, d.SalesOrderHeader.OrderDate }).ToList();
				var expected = offers.FullJoin(details,
								so => new { so.SpecialOfferId, so.ProductId },
								d => new { d.SpecialOfferId, d.ProductId },
								(so, d) => new
								{
									OfferProductId = so == null ? (int?)null : so.ProductId,
									ProductName = so == null ? null : so.Name,
									DetailId = d == null ? (int?)null : d.SalesOrderDetailId,
									OrderDate = d == null ? (DateTime?)null : d.OrderDate
								})
								.OrderBy(x => x.OfferProductId).ThenBy(x => x.DetailId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
				// offer products not ordered in the range: preserved rows whose null-side navigation yields null...
				Assert.IsTrue(actual.Any(x => x.DetailId == null && x.OrderDate == null && x.ProductName != null),
					"Expected left-only rows with a working preserved-side navigation and a null defaulted-side navigation");
			}
		}


		[Test]
		public void FullJoinWithCustomComparerThrowsNotSupported()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products
								.FullJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId,
									(p, d) => new { ProductId = (int?)p.ProductId, DetailId = (int?)d.SalesOrderDetailId },
									EqualityComparer<int>.Default);

				Assert.Throws<NotSupportedException>(() => query.ToList());
			}
		}


		[Test]
		public void FullJoinTupleOverloadThrowsNotSupported()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products
								.FullJoin(ctx.SalesOrderDetails,
									p => p.ProductId,
									d => d.ProductId);

				Assert.Throws<NotSupportedException>(() => query.ToList());
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

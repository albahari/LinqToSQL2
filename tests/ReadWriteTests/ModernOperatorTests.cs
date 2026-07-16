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
	/// Tests for the sequence operators introduced in .NET 6+ (Order/OrderDescending, MinBy/MaxBy,
	/// ExceptBy/IntersectBy) and the ElementAt/ElementAtOrDefault operators.
	/// </summary>
	[TestFixture]
	public class ModernOperatorTests
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
		public void OrderMatchesOrderByIdentity()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Select(p => p.Name).Order().ToList();
				var expected = ctx.Products.Select(p => p.Name).OrderBy(n => n).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void OrderDescendingMatchesOrderByDescendingIdentity()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.SalesOrderDetails.Select(d => d.OrderQty).OrderDescending().ToList();
				var expected = ctx.SalesOrderDetails.Select(d => d.OrderQty).OrderByDescending(q => q).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void OrderComposesWithTake()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Select(p => p.Name).Order().Take(10).ToList();
				var expected = ctx.Products.Select(p => p.Name).OrderBy(n => n).Take(10).ToList();

				Assert.AreEqual(10, actual.Count);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void MinByReturnsEntityWithMinimalKey()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.MinBy(p => p.ListPrice);

				Assert.IsNotNull(actual);
				Assert.AreEqual(ctx.Products.Min(p => p.ListPrice), actual.ListPrice);
			}
		}


		[Test]
		public void MaxByReturnsEntityWithMaximalKey()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.MaxBy(p => p.ListPrice);

				Assert.IsNotNull(actual);
				Assert.AreEqual(ctx.Products.Max(p => p.ListPrice), actual.ListPrice);
			}
		}


		[Test]
		public void MinByOnEmptySourceReturnsNull()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Where(p => p.ProductId < 0).MinBy(p => p.ListPrice);

				Assert.IsNull(actual);
			}
		}


		[Test]
		public void MinByOnScalarSequence()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Select(p => p.ListPrice).MinBy(x => x);

				Assert.AreEqual(ctx.Products.Min(p => p.ListPrice), actual);
			}
		}


		[Test]
		public void MaxByNestedInProjection()
		{
			using(var ctx = GetContext())
			{
				var actual = (from sc in ctx.ProductSubcategories
							  orderby sc.ProductSubcategoryId
							  select new { sc.Name, PriciestProduct = sc.Products.MaxBy(p => p.ListPrice).Name })
							  .ToList();

				var expected = (from sc in ctx.ProductSubcategories
								orderby sc.ProductSubcategoryId
								select new { sc.Name, PriciestProduct = sc.Products.OrderByDescending(p => p.ListPrice).FirstOrDefault().Name })
								.ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void ElementAtMatchesSkipFirst()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.ProductId).ElementAt(5);
				var expected = ctx.Products.OrderBy(p => p.ProductId).Skip(5).First();

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.ProductId, actual.ProductId);
			}
		}


		[Test]
		public void ElementAtOrDefaultOutOfRangeReturnsNull()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.ProductId).ElementAtOrDefault(10000000);

				Assert.IsNull(actual);
			}
		}


		[Test]
		public void ElementAtOutOfRangeThrows()
		{
			using(var ctx = GetContext())
			{
				// NB: the BCL operator throws ArgumentOutOfRangeException; the Skip(n).First() translation
				// surfaces this as First's InvalidOperationException instead.
				Assert.Throws<InvalidOperationException>(
					() => ctx.Products.OrderBy(p => p.ProductId).ElementAt(10000000));
			}
		}


		[Test]
		public void ExceptByWithQueryableKeySource()
		{
			using(var ctx = GetContext())
			{
				// products which were never ordered.
				var soldProductIds = ctx.SalesOrderDetails.Select(d => d.ProductId);

				var actual = ctx.Products.ExceptBy(soldProductIds, p => p.ProductId)
										 .Select(p => p.ProductId)
										 .ToList();

				var expected = ctx.Products.Where(p => !soldProductIds.Contains(p.ProductId))
										   .Select(p => p.ProductId)
										   .ToList();

				actual.Sort();
				expected.Sort();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void IntersectByWithLocalKeyCollection()
		{
			using(var ctx = GetContext())
			{
				var ids = new[] { 1, 2, 3, 999999 };

				var actual = ctx.Products.IntersectBy(ids, p => p.ProductId)
										 .Select(p => p.ProductId)
										 .ToList();

				var expected = ctx.Products.Where(p => ids.Contains(p.ProductId))
										   .Select(p => p.ProductId)
										   .ToList();

				actual.Sort();
				expected.Sort();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void ExceptByWithLocalKeyCollection()
		{
			using(var ctx = GetContext())
			{
				var ids = new[] { 1, 2, 3 };

				var actualCount = ctx.Products.ExceptBy(ids, p => p.ProductId).Count();
				var expectedCount = ctx.Products.Count(p => !ids.Contains(p.ProductId));

				Assert.Greater(actualCount, 0);
				Assert.AreEqual(expectedCount, actualCount);
			}
		}


		[Test]
		public void IntersectByWithCompositeKey()
		{
			using(var ctx = GetContext())
			{
				// sales order details whose (SpecialOfferId, ProductId) pair occurs in SpecialOfferProducts: all of them (FK).
				var pairs = ctx.SpecialOfferProducts.Select(so => new { so.SpecialOfferId, so.ProductId });

				var detailsQuery = ctx.SalesOrderDetails.Where(d => d.SalesOrderId < 43700);
				var actualCount = detailsQuery.IntersectBy(pairs, d => new { d.SpecialOfferId, d.ProductId }).Count();

				Assert.Greater(actualCount, 0);
				Assert.AreEqual(detailsQuery.Count(), actualCount);
			}
		}


		[Test]
		public void DistinctByReturnsOneRowPerKey()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.DistinctBy(p => p.Color).ToList();
				var expectedKeys = ctx.Products.Select(p => p.Color).Distinct().ToList();

				Assert.AreEqual(expectedKeys.Count, actual.Count);
				CollectionAssert.AreEquivalent(expectedKeys, actual.Select(p => p.Color));
			}
		}


		[Test]
		public void DistinctByComposesWithCount()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.DistinctBy(p => p.Color).Count();
				var expected = ctx.Products.Select(p => p.Color).Distinct().Count();

				Assert.Greater(actual, 0);
				Assert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void UnionByYieldsOneRowPerKeyAcrossBothSequences()
		{
			using(var ctx = GetContext())
			{
				var first = ctx.Products.Where(p => p.ListPrice > 1000);
				var second = ctx.Products.Where(p => p.ListPrice > 500);

				var actual = first.UnionBy(second, p => p.Color).ToList();
				var expectedKeys = first.Select(p => p.Color).Union(second.Select(p => p.Color)).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEquivalent(expectedKeys, actual.Select(p => p.Color));
			}
		}


		[Test]
		public void CountByMatchesGroupByWithCount()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.CountBy(p => p.Color).ToList();
				var expected = ctx.Products.GroupBy(p => p.Color).Select(g => new { g.Key, Count = g.Count() }).ToList();

				CollectionAssert.IsNotEmpty(actual);
				Assert.AreEqual(expected.Count, actual.Count);
				foreach(var kvp in actual)
				{
					Assert.AreEqual(expected.Single(x => x.Key == kvp.Key).Count, kvp.Value);
				}
			}
		}


		[Test]
		public void ShuffleReturnsAllRowsAndOrdersByNewid()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products.Shuffle().Select(p => p.ProductId);

				StringAssert.Contains("NEWID", ctx.GetCommand(query).CommandText);

				var actual = query.ToList();
				var expected = ctx.Products.Select(p => p.ProductId).ToList();
				CollectionAssert.AreEquivalent(expected, actual);
			}
		}


		[Test]
		public void ShuffleComposesWithTake()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Shuffle().Take(5).ToList();

				Assert.AreEqual(5, actual.Count);
				Assert.AreEqual(5, actual.Select(p => p.ProductId).Distinct().Count());
			}
		}


		[Test]
		public void LastReturnsFinalElementOfOrderedSequence()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.ProductId).Last();
				var expected = ctx.Products.OrderByDescending(p => p.ProductId).First();

				Assert.IsNotNull(actual);
				Assert.AreEqual(expected.ProductId, actual.ProductId);
			}
		}


		[Test]
		public void LastWithPredicate()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.ProductId).Last(p => p.ListPrice > 100);
				var expected = ctx.Products.OrderByDescending(p => p.ProductId).First(p => p.ListPrice > 100);

				Assert.AreEqual(expected.ProductId, actual.ProductId);
			}
		}


		[Test]
		public void LastFindsOrderingBelowIntermediateOperators()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.Name).ThenBy(p => p.ProductId).Where(p => p.ListPrice > 100).Last();
				var expected = ctx.Products.OrderByDescending(p => p.Name).ThenByDescending(p => p.ProductId).Where(p => p.ListPrice > 100).First();

				Assert.AreEqual(expected.ProductId, actual.ProductId);
			}
		}


		[Test]
		public void LastOrDefaultOnEmptySourceReturnsNull()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.Where(p => p.ProductId < 0).OrderBy(p => p.ProductId).LastOrDefault();

				Assert.IsNull(actual);
			}
		}


		[Test]
		public void LastWithoutOrderingThrows()
		{
			using(var ctx = GetContext())
			{
				Assert.Throws<NotSupportedException>(() => ctx.Products.Last());
			}
		}


		[Test]
		public void LastAfterTakeThrows()
		{
			using(var ctx = GetContext())
			{
				Assert.Throws<NotSupportedException>(() => ctx.Products.OrderBy(p => p.ProductId).Take(5).Last());
			}
		}


		[Test]
		public void LastAfterSkipThrows()
		{
			using(var ctx = GetContext())
			{
				Assert.Throws<NotSupportedException>(() => ctx.Products.OrderBy(p => p.ProductId).Skip(5).Last());
			}
		}


		[Test]
		public void ReverseInvertsOrdering()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.OrderBy(p => p.Name).ThenBy(p => p.ProductId).Reverse()
										 .Select(p => p.ProductId).ToList();
				var expected = ctx.Products.OrderByDescending(p => p.Name).ThenByDescending(p => p.ProductId)
										   .Select(p => p.ProductId).ToList();

				CollectionAssert.IsNotEmpty(actual);
				CollectionAssert.AreEqual(expected, actual);
			}
		}


		[Test]
		public void ReverseWithoutOrderingThrows()
		{
			using(var ctx = GetContext())
			{
				Assert.Throws<NotSupportedException>(() => ctx.Products.Reverse().ToList());
			}
		}


		[Test]
		public void TagWithEmitsLeadingComment()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products.TagWith("Fetch products").Where(p => p.ListPrice > 0);

				StringAssert.StartsWith("-- Fetch products", ctx.GetCommand(query).CommandText);
				CollectionAssert.IsNotEmpty(query.ToList());
			}
		}


		[Test]
		public void TagWithAccumulatesMultipleTags()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products.TagWith("first tag").Where(p => p.ListPrice > 0).TagWith("second tag");
				string sql = ctx.GetCommand(query).CommandText;

				StringAssert.Contains("-- first tag", sql);
				StringAssert.Contains("-- second tag", sql);
			}
		}


		[Test]
		public void TagWithPrefixesEveryLineOfMultilineTag()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products.TagWith("line one\nline two");
				string sql = ctx.GetCommand(query).CommandText;

				StringAssert.Contains("-- line one", sql);
				StringAssert.Contains("-- line two", sql);
				Assert.IsFalse(sql.Split("\r\n").Any(l => l.StartsWith("line")), "Every tag line must be commented out");
			}
		}


		[Test]
		public void StringJoinOverGroupTranslatesToStringAgg()
		{
			using(var ctx = GetContext())
			{
				var query = ctx.Products.GroupBy(p => p.Color)
									    .Select(g => new { g.Key, Names = string.Join("|", g.Select(x => x.Name)) });

				StringAssert.Contains("STRING_AGG", ctx.GetCommand(query).CommandText);

				var actual = query.ToList();
				var expected = ctx.Products.Select(p => new { p.Color, p.Name }).ToList()
										   .GroupBy(x => x.Color)
										   .ToDictionary(g => g.Key ?? "<null>", g => g.Select(x => x.Name).OrderBy(n => n).ToList());

				Assert.AreEqual(expected.Count, actual.Count);
				foreach(var row in actual)
				{
					// SQL concatenates in arbitrary order, so compare as sorted sets.
					CollectionAssert.AreEqual(expected[row.Key ?? "<null>"], row.Names.Split('|').OrderBy(n => n).ToList());
				}
			}
		}


		[Test]
		public void StringJoinOverGroupWithElementSelector()
		{
			using(var ctx = GetContext())
			{
				// exercises the raw-grouping path (no Select inside the aggregate).
				var actual = ctx.Products.GroupBy(p => p.Color, p => p.Name)
										 .Select(g => new { g.Key, Names = string.Join("|", g) })
										 .ToList();

				var expected = ctx.Products.Select(p => new { p.Color, p.Name }).ToList()
										   .GroupBy(x => x.Color)
										   .ToDictionary(g => g.Key ?? "<null>", g => g.Select(x => x.Name).OrderBy(n => n).ToList());

				Assert.AreEqual(expected.Count, actual.Count);
				foreach(var row in actual)
				{
					CollectionAssert.AreEqual(expected[row.Key ?? "<null>"], row.Names.Split('|').OrderBy(n => n).ToList());
				}
			}
		}


		[Test]
		public void StringJoinTreatsNullElementsAsEmptyStrings()
		{
			using(var ctx = GetContext())
			{
				// Color is null for many products: string.Join renders those as empty strings.
				var actual = ctx.Products.GroupBy(p => p.ProductSubcategoryId)
										 .Select(g => new { g.Key, Colors = string.Join("|", g.Select(x => x.Color)) })
										 .ToList();

				var expected = ctx.Products.Select(p => new { p.ProductSubcategoryId, p.Color }).ToList()
										   .GroupBy(x => x.ProductSubcategoryId)
										   .ToDictionary(g => g.Key?.ToString() ?? "<null>",
													     g => g.Select(x => x.Color ?? "").OrderBy(c => c).ToList());

				Assert.AreEqual(expected.Count, actual.Count);
				foreach(var row in actual)
				{
					CollectionAssert.AreEqual(expected[row.Key?.ToString() ?? "<null>"], row.Colors.Split('|').OrderBy(c => c).ToList());
				}
			}
		}


		[Test]
		public void StringJoinOverRelatedEntitiesUsesCorrelatedSubquery()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.ProductSubcategories
								.Select(sc => new { sc.Name, Products = string.Join("|", sc.Products.Select(p => p.Name)) })
								.ToList();

				var expected = ctx.Products.Where(p => p.ProductSubcategoryId != null)
										   .Select(p => new { p.ProductSubcategoryId, p.Name }).ToList()
										   .GroupBy(x => x.ProductSubcategoryId.Value)
										   .ToDictionary(g => g.Key, g => g.Select(x => x.Name).OrderBy(n => n).ToList());
				var subcategoryNames = ctx.ProductSubcategories
										  .Select(sc => new { sc.ProductSubcategoryId, sc.Name }).ToList()
										  .ToDictionary(x => x.Name, x => x.ProductSubcategoryId);

				CollectionAssert.IsNotEmpty(actual);
				foreach(var row in actual)
				{
					CollectionAssert.AreEqual(expected[subcategoryNames[row.Name]], row.Products.Split('|').OrderBy(n => n).ToList());
				}
			}
		}


		[Test]
		public void StringJoinOverEmptySequenceYieldsEmptyString()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.ProductSubcategories
								.Select(sc => string.Join("|", sc.Products.Where(p => p.ProductId < 0).Select(p => p.Name)))
								.ToList();

				CollectionAssert.IsNotEmpty(actual);
				Assert.IsTrue(actual.All(s => s == ""), "string.Join over an empty sequence must yield an empty string");
			}
		}


		[Test]
		public void StringJoinWithNonStringElementsAndCharSeparator()
		{
			using(var ctx = GetContext())
			{
				var actual = ctx.Products.GroupBy(p => p.Color)
										 .Select(g => new { g.Key, Ids = string.Join('|', g.Select(x => x.ProductId)) })
										 .ToList();

				var expected = ctx.Products.Select(p => new { p.Color, p.ProductId }).ToList()
										   .GroupBy(x => x.Color)
										   .ToDictionary(g => g.Key ?? "<null>",
													     g => g.Select(x => x.ProductId.ToString()).OrderBy(i => i).ToList());

				Assert.AreEqual(expected.Count, actual.Count);
				foreach(var row in actual)
				{
					CollectionAssert.AreEqual(expected[row.Key ?? "<null>"], row.Ids.Split('|').OrderBy(i => i).ToList());
				}
			}
		}


		[Test]
		public void StringJoinWithOrderedElementsThrows()
		{
			using(var ctx = GetContext())
			{
				Assert.Throws<NotSupportedException>(() =>
					ctx.Products.GroupBy(p => p.Color)
								.Select(g => string.Join("|", g.OrderBy(x => x.Name).Select(x => x.Name)))
								.ToList());
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

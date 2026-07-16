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

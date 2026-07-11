using Xunit;
using System;
using System.Linq;
using System.Data.Linq.SqlClient;

namespace NewTypeTests
{
	public class JsonTests : IClassFixture<JsonDatabaseFixture>
	{
		JsonDatabaseFixture _fixture;
		TypedDataContext Data => _fixture.Data;

		public JsonTests (JsonDatabaseFixture fixture) => _fixture = fixture;

		[Fact]
		public void BasicSelectWorks ()
		{
			var row = Data.JsonTests.Single (j => j.ID == 1);
			Assert.Contains ("\"name\":\"alice\"", row.Doc);   // the json type stores documents in normalized form
		}

		[Fact]
		public void NullRoundTrips () => Assert.Null (Data.JsonTests.Single (j => j.ID == 2).Doc);

		[Fact]
		public void CanInsertUpdateDelete ()
		{
			var newRow = new JsonTest { ID = 3, Doc = """{"x": 1}""" };
			Data.JsonTests.InsertOnSubmit (newRow);
			Data.SubmitChanges ();

			using (var freshContext = new TypedDataContext ())
			{
				var fetched = freshContext.JsonTests.Single (j => j.ID == 3);
				Assert.Equal ("""{"x":1}""", fetched.Doc);

				fetched.Doc = """{"x": 2}""";
				freshContext.SubmitChanges ();
			}

			using (var freshContext = new TypedDataContext ())
			{
				var fetched = freshContext.JsonTests.Single (j => j.ID == 3);
				Assert.Equal ("""{"x":2}""", fetched.Doc);

				freshContext.JsonTests.DeleteOnSubmit (fetched);
				freshContext.SubmitChanges ();
			}

			Assert.Equal (2, Data.JsonTests.Count ());
		}

		[Fact]
		public void JsonValueInPredicate () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonValue (j.Doc!, ("$.name")) == "alice"));

		[Fact]
		public void JsonValueInProjection () =>
			Assert.Equal ("Perth", Data.JsonTests.Where (j => j.ID == 1).Select (j => SqlMethods.JsonValue (j.Doc, "$.address.city")).Single ());

		[Fact]
		public void JsonValueTranslatesToServerFunction ()
		{
			var query = Data.JsonTests.Select (j => SqlMethods.JsonValue (j.Doc!, "$.name"));
			Assert.Contains ("JSON_VALUE", Data.GetCommand (query).CommandText);
		}

		[Fact]
		public void JsonQueryReturnsFragment () =>
			Assert.Equal ("[1,2,3]", Data.JsonTests.Where (j => j.ID == 1).Select (j => SqlMethods.JsonQuery (j.Doc!, "$.tags")).Single ());

		[Fact]
		public void IsJsonInPredicate () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.IsJson (j.Doc)));   // null document yields NULL, excluding the row

		[Fact]
		public void JsonPathExistsInPredicate () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonPathExists(j.Doc, "$.address.city")));

		[Fact]
		public void JsonModifyInProjection ()
		{
			var modified = Data.JsonTests.Where (j => j.ID == 1).Select (j => SqlMethods.JsonModify(j.Doc, "$.name", "bob")).Single ();
			Assert.Contains ("\"name\":\"bob\"", modified);
		}

		[Fact]
		public void JsonContainsStringCandidate () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, "alice", "$.name")));

		[Fact]
		public void JsonContainsNumericCandidateInArray () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, 2, "$.tags[*]")));

		[Fact]
		public void JsonContainsRespectsSqlType ()
		{
			// The comparison uses the SQL type of the candidate, so the string "2" doesn't match the number 2
			Assert.Equal (0, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, "2", "$.tags[*]")));
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, 10.5m, "$.price")));
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, true, "$.active")));
		}

		[Fact]
		public void JsonContainsLikeSearchMode () =>
			Assert.Equal (1, Data.JsonTests.Count (j => SqlMethods.JsonContains (j.Doc!, "ali%", "$.name", 1)));

		[Fact]
		public void JsonContainsTranslatesToServerFunction ()
		{
			var query = Data.JsonTests.Where (j => SqlMethods.JsonContains (j.Doc!, "alice", "$.name")).Select (j => j.ID);
			Assert.Contains ("JSON_CONTAINS", Data.GetCommand (query).CommandText);
		}

		[Fact]
		public void JsonMethodsThrowOnClient () =>
			Assert.Throws<NotSupportedException> (() => SqlMethods.JsonValue ("{}", "$.x"));
	}
}

using Xunit;
using System;
using System.Linq;
using System.Data.Linq.SqlClient;

namespace NewTypeTests
{
	public class VectorTests : IClassFixture<VectorDatabaseFixture>
	{
		static readonly float[] Target = { 1, 2, 3 };

		VectorDatabaseFixture _fixture;
		TypedDataContext Data => _fixture.Data;

		public VectorTests (VectorDatabaseFixture fixture) => _fixture = fixture;

		[Fact]
		public void BasicSelectWorks ()
		{
			var row = Data.VectorTests.Single (v => v.ID == 1);
			Assert.Equal (new float[] { 1, 2, 3 }, row.Vec);
		}

		[Fact]
		public void NullRoundTrips () => Assert.Null (Data.VectorTests.Single (v => v.ID == 2).Vec);

		[Fact]
		public void ProjectionWorks () =>
			Assert.Equal (new float[] { 1, 2, 3 }, Data.VectorTests.Where (v => v.ID == 1).Select (v => v.Vec).Single ());

		[Fact]
		public void CanInsertUpdateDelete ()
		{
			// Fractional and small values exercise the text round-trip (server sends scientific notation)
			var newRow = new VectorTest { ID = 3, Vec = new[] { 7.5f, -0.001f, 42f } };
			Data.VectorTests.InsertOnSubmit (newRow);
			Data.SubmitChanges ();

			using (var freshContext = new TypedDataContext ())
			{
				var fetched = freshContext.VectorTests.Single (v => v.ID == 3);
				Assert.Equal (newRow.Vec, fetched.Vec);

				fetched.Vec = new[] { 1e10f, 1.5e-30f, 0f };
				freshContext.SubmitChanges ();
			}

			using (var freshContext = new TypedDataContext ())
			{
				var fetched = freshContext.VectorTests.Single (v => v.ID == 3);
				Assert.Equal (new[] { 1e10f, 1.5e-30f, 0f }, fetched.Vec);

				freshContext.VectorTests.DeleteOnSubmit (fetched);
				freshContext.SubmitChanges ();
			}

			Assert.Equal (2, Data.VectorTests.Count ());
		}

		[Fact]
		public void VectorDistanceInPredicate () =>
			// Row 1 is [1,2,3] (distance ~0 from Target); the null row yields NULL, excluding it
			Assert.Equal (1, Data.VectorTests.Count (v => v.Vec!.VectorDistance ("cosine", Target) < 0.0001));

		[Fact]
		public void VectorDistanceInProjection ()
		{
			var distance = Data.VectorTests.Where (v => v.ID == 1).Select (v => v.Vec!.VectorDistance ("euclidean", Target)).Single ();
			Assert.True (distance < 0.001, $"distance was {distance}");
		}

		[Fact]
		public void VectorDistanceBetweenColumns () =>
			Assert.Equal (1, Data.VectorTests.Count (v => v.Vec!.VectorDistance ("cosine", v.Vec!) < 0.0001));

		[Fact]
		public void VectorDistanceTranslatesToServerFunction ()
		{
			var query = Data.VectorTests.Select (v => v.Vec!.VectorDistance ("cosine", Target));
			var commandText = Data.GetCommand (query).CommandText;
			Assert.Contains ("VECTOR_DISTANCE", commandText);
			Assert.Contains ("vector(3)", commandText);   // client operand must be CONVERTed with the column's dimension
		}

		[Fact]
		public void VectorDistanceInfersDimensionFromClientValue () =>
			// VectorTest2 maps the column as dimensionless "Vector", so the client operand's length supplies it
			Assert.Equal (1, Data.VectorTest2s.Count (v => v.Vec!.VectorDistance ("cosine", Target) < 0.0001));

		[Fact]
		public void VectorDistanceThrowsOnClient () =>
			Assert.Throws<NotSupportedException> (() => new float[] { 1 }.VectorDistance ("cosine", new float[] { 1 }));

		[Fact]
		public void CanSetToNullAndBack ()
		{
			var row = Data.VectorTests.Single (v => v.ID == 1);
			var original = row.Vec;

			row.Vec = null;
			Data.SubmitChanges ();
			using (var freshContext = new TypedDataContext ())
				Assert.Null (freshContext.VectorTests.Single (v => v.ID == 1).Vec);

			row.Vec = original;
			Data.SubmitChanges ();
			using (var freshContext = new TypedDataContext ())
				Assert.Equal (original, freshContext.VectorTests.Single (v => v.ID == 1).Vec);
		}
	}
}

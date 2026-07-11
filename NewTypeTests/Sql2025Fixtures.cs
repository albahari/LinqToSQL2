using System;

namespace NewTypeTests
{
	/// <summary>
	/// Base for fixtures that exercise SQL Server 2025+ types (json, vector).
	/// </summary>
	public abstract class Sql2025DatabaseFixture : IDisposable
	{
		public readonly TypedDataContext Data;

		protected Sql2025DatabaseFixture (string setupSql)
		{
			if (TestConfig.EnsureDatabase () < 17)
				throw new InvalidOperationException (
					"These tests require SQL Server 2025 or later. Point L2S_NEWTYPETESTS_CONN at a " +
					@"2025+ server, or leave it unset to use (localdb)\MSSQLLocalDB2025.");

			Data = new TypedDataContext ();
			Data.ExecuteCommand (setupSql);
		}

		public void Dispose () => Data.Dispose ();
	}

	public class JsonDatabaseFixture : Sql2025DatabaseFixture
	{
		public JsonDatabaseFixture () : base ("""
			drop table if exists JsonTest
			create table JsonTest (ID int not null primary key, Doc json null)
			insert JsonTest (ID, Doc) values
			(1, '{"name": "alice", "tags": [1, 2, 3], "address": {"city": "Perth"}, "price": 10.5, "active": true}'),
			(2, null)
			""") { }
	}

	public class VectorDatabaseFixture : Sql2025DatabaseFixture
	{
		public VectorDatabaseFixture () : base ("""
			drop table if exists VectorTest
			create table VectorTest (ID int not null primary key, Vec vector(3) null)
			insert VectorTest (ID, Vec) values
			(1, '[1, 2, 3]'),
			(2, null)
			""") { }
	}
}

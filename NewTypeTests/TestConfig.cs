using System;
using System.Data.Linq.DbEngines.SqlServer;
using Microsoft.Data.SqlClient;

namespace NewTypeTests
{
	/// <summary>
	/// Central place for the test connection string. The default targets the SQL Server 2025 LocalDB
	/// instance so that the json tests can run; override via the L2S_NEWTYPETESTS_CONN environment
	/// variable to point elsewhere. The database is created on first use if it doesn't exist.
	/// </summary>
	internal static class TestConfig
	{
		public static readonly string ConnectionString =
			Environment.GetEnvironmentVariable ("L2S_NEWTYPETESTS_CONN")
			?? @"Data Source=(localdb)\MSSQLLocalDB2025;Initial Catalog=L2SNewTypeTests;Integrated Security=SSPI;TrustServerCertificate=true";

		static readonly Lazy<int> _serverMajorVersion = new (() =>
		{
			SqlLibrary.PreferMicrosoftDataClient = true;

			var builder = new SqlConnectionStringBuilder (ConnectionString) { ConnectTimeout = 5 };
			string database = builder.InitialCatalog;
			builder.InitialCatalog = "master";

			using var cn = new SqlConnection (builder.ConnectionString);
			cn.Open ();
			using var cmd = new SqlCommand (
				$"if db_id (N'{database}') is null create database [{database}]; " +
				"select cast (serverproperty ('ProductMajorVersion') as int)", cn);
			return (int) cmd.ExecuteScalar ()!;
		});

		/// <summary>
		/// Creates the test database if necessary and returns the server's major version
		/// (17 = SQL Server 2025, the first version with the native json type).
		/// Call this before constructing a TypedDataContext.
		/// </summary>
		public static int EnsureDatabase () => _serverMajorVersion.Value;
	}
}

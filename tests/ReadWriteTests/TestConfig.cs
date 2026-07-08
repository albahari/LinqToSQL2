using System;
using System.Data.SqlClient;

namespace ReadWriteTests.SqlServer
{
	/// <summary>
	/// Central place for the test connection strings. Defaults target a local SQL Server; override via
	/// the L2S_ADVENTUREWORKS_CONN / L2S_WRITETESTS_CONN environment variables to point elsewhere.
	///
	/// NB: the AdventureWorks model targets the *redesigned* Person/BusinessEntity schema introduced in
	/// AdventureWorks 2008R2 (and carried through 2012-2022). The original AdventureWorks2008 OLTP sample
	/// (with Person.Contact and no Customer.AccountNumber) will not satisfy these tests.
	/// </summary>
	internal static class TestConfig
	{
		public static string AdventureWorks =>
			Environment.GetEnvironmentVariable("L2S_ADVENTUREWORKS_CONN")
			?? "Server=localhost;Database=AdventureWorks2012;Integrated Security=true;TrustServerCertificate=true";

		public static string WriteTests =>
			Environment.GetEnvironmentVariable("L2S_WRITETESTS_CONN")
			?? "Server=localhost;Database=LLBLGenProUnitTest;Integrated Security=true;TrustServerCertificate=true";

		/// <summary>
		/// Connection string for the BigJoinOrderingTests fixture, which creates and seeds its own
		/// database (see that fixture's OneTimeSetUp). Only the server part needs to be reachable.
		/// </summary>
		public static string BigJoin =>
			Environment.GetEnvironmentVariable("L2S_BIGJOIN_CONN")
			?? "Server=localhost;Database=L2SBigJoinTests;Integrated Security=true;TrustServerCertificate=true";

		/// <summary>Returns true if a connection to the given connection string can be opened.</summary>
		public static bool CanConnect(string connectionString)
		{
			try
			{
				// Force a short connect timeout so a missing database fails fast instead of
				// stalling the whole run on the default login timeout.
				var builder = new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 3 };
				using var cn = new SqlConnection(builder.ConnectionString);
				cn.Open();
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}

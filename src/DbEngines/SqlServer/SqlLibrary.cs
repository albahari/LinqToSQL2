using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Globalization;

namespace System.Data.Linq.DbEngines.SqlServer
{
	public abstract class SqlLibrary
	{
		public static bool PreferMicrosoftDataClient { get; set; }

		static SqlLibrary _preferredInstance;
		public static SqlLibrary PreferredInstance => _preferredInstance ??=
			PreferMicrosoftDataClient && MicrosoftSqlLibrary.Instance != null 
				? (SqlLibrary) MicrosoftSqlLibrary.Instance 
				: SystemSqlLibrary.Instance;

		public static SqlLibrary FromConnection (DbConnection cx) => FromDbObject(cx);
		public static SqlLibrary FromDbParameter (DbParameter p) => FromDbObject(p);
		static SqlLibrary FromDbObject (object obj)
		{
			if (obj.GetType().Namespace == "System.Data.SqlClient")
				return SystemSqlLibrary.Instance;

			if (obj.GetType().Namespace == "System.Data.SqlServerCe" && CeSqlLibrary.Instance.DbProviderFactory != null)
				return CeSqlLibrary.Instance;

#if NET6_0_OR_GREATER
			if (obj.GetType().Namespace == "Microsoft.Data.SqlClient")
				return MicrosoftSqlLibrary.Instance;
#endif
			return null;
		}

		internal static void PopulateDbParameter (DbParameter parameter, SqlType sqlType)
		{
			var lib = FromDbParameter (parameter);
			if (lib != null && lib.PopulateDbParameterCore (parameter, sqlType))
				return;

			PropertyInfo piSqlDbType = parameter.GetType().GetProperty("SqlDbType");
			if (piSqlDbType != null)
			{
				piSqlDbType.SetValue(parameter, sqlType.SqlDbType, null);
			}
			if (sqlType.HasPrecisionAndScale)
			{
				PropertyInfo piPrecision = parameter.GetType().GetProperty("Precision");
				if (piPrecision != null)
				{
					piPrecision.SetValue(parameter, Convert.ChangeType(sqlType.Precision, piPrecision.PropertyType, CultureInfo.InvariantCulture), null);
				}
				PropertyInfo piScale = parameter.GetType().GetProperty("Scale");
				if (piScale != null)
				{
					piScale.SetValue(parameter, Convert.ChangeType(sqlType.Scale, piScale.PropertyType, CultureInfo.InvariantCulture), null);
				}
			}
		}

		public abstract DbProviderFactory DbProviderFactory { get; }		

		public abstract void ClearAllPools();

		internal abstract Type GetDataReaderType ();

		internal virtual void WriteSqlParameter (TextWriter writer, DbParameter p, int prec, int scale) =>
			WriteSqlParameter (writer, p.ParameterName, p.Direction, p.DbType.ToString(), p.Size, prec, scale, p.Value);

		protected void WriteSqlParameter (TextWriter writer, string name, ParameterDirection direction, string type, int size, int prec, int scale, object value) =>
			writer.WriteLine("-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]",
				name, direction, type, size, prec, scale, value);

		internal virtual bool PopulateDbParameterCore (DbParameter parameter, SqlType sqlType) => false;

		public virtual DbConnection CreateConnection () => DbProviderFactory.CreateConnection();

		//DbCommandBuilder _dbCommandBuilder;
		//public DbCommandBuilder GetDbCommandBuilder() => _dbCommandBuilder ??= DbProviderFactory.CreateCommandBuilder();

		public static string QuoteIdentifier (string unquotedIdentifier)
		{
			if (string.IsNullOrEmpty(unquotedIdentifier)) return unquotedIdentifier;
			return "[" + unquotedIdentifier + "]";
        }
	}

	class CeSqlLibrary : SqlLibrary
	{
		const string SqlCeDataReaderTypeName = "System.Data.SqlServerCe.SqlCeDataReader";

		public static readonly CeSqlLibrary Instance = new CeSqlLibrary();

		public static DbProviderFactory Configure (DbProviderFactory factory) => _factory = factory;

		static DbProviderFactory _factory;
		public override DbProviderFactory DbProviderFactory => _factory;

		public override void ClearAllPools ()
		{
		}

		internal override Type GetDataReaderType () => DbProviderFactory.GetType().Module.GetType(SqlCeDataReaderTypeName);
	}
}

namespace System.Data.Linq.DbEngines.SqlServer
{
    using System.Data.SqlClient;

	class SystemSqlLibrary : SqlLibrary
	{
		public static readonly SystemSqlLibrary Instance = new SystemSqlLibrary();

		public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

		public override void ClearAllPools ()
		{
			SqlConnection.ClearAllPools();
		}

		internal override Type GetDataReaderType () => typeof(SqlDataReader);

		internal override bool PopulateDbParameterCore (DbParameter parameter, SqlType sqlType)
		{
			SqlParameter sParameter = parameter as SqlParameter;
			if (sParameter == null) return false;

			sParameter.SqlDbType = sqlType.SqlDbType;
			if (sqlType.HasPrecisionAndScale)
			{
				sParameter.Precision = (byte)sqlType.Precision;
				sParameter.Scale = (byte)sqlType.Scale;
			}
			return true;
		}

		internal override void WriteSqlParameter (TextWriter writer, DbParameter p, int prec, int scale)
		{ 
			var sp = p as SqlParameter;
			if (sp == null) 
				base.WriteSqlParameter(writer, p, prec, scale);
			else
				WriteSqlParameter(writer, p.ParameterName, p.Direction, sp.SqlDbType.ToString(), p.Size, prec, scale, sp.SqlValue);
		}
	}
}

namespace System.Data.Linq.DbEngines.SqlServer
{
	using Microsoft.Data.SqlClient;

	class MicrosoftSqlLibrary : SqlLibrary
	{
		public static readonly MicrosoftSqlLibrary Instance = new MicrosoftSqlLibrary();

		public override DbProviderFactory DbProviderFactory => SqlClientFactory.Instance;

		public override void ClearAllPools ()
		{
			SqlConnection.ClearAllPools();
		}

		internal override Type GetDataReaderType () => typeof(SqlDataReader);

		internal override bool PopulateDbParameterCore (DbParameter parameter, SqlType sqlType)
		{
			SqlParameter sParameter = parameter as SqlParameter;
			if (sParameter == null) return false;

			sParameter.SqlDbType = sqlType.SqlDbType;
			if (sqlType.HasPrecisionAndScale)
			{
				sParameter.Precision = (byte)sqlType.Precision;
				sParameter.Scale = (byte)sqlType.Scale;
			}
			return true;
		}

		internal override void WriteSqlParameter (TextWriter writer, DbParameter p, int prec, int scale)
		{
			var sp = p as SqlParameter;
			if (sp == null)
				base.WriteSqlParameter(writer, p, prec, scale);
			else
				WriteSqlParameter(writer, p.ParameterName, p.Direction, sp.SqlDbType.ToString(), p.Size, prec, scale, sp.SqlValue);
		}
	}
}

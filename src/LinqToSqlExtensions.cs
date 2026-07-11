using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

#if NET6_0_OR_GREATER
namespace System.Data.Linq
{
	public static class LinqToSqlExtensions
	{
		// These extension methods make it easier for users to compare dates with datetimes.

		public static DateOnly DateOnly (this DateTime dateTime) => System.DateOnly.FromDateTime(dateTime);
		public static DateOnly? DateOnly (this DateTime? dateTime) => dateTime == null ? null : System.DateOnly.FromDateTime(dateTime.Value);

		public static TimeOnly TimeOnly (this DateTime dateTime) => System.TimeOnly.FromDateTime(dateTime);
		public static TimeOnly? TimeOnly (this DateTime? dateTime) => dateTime == null ? null : System.TimeOnly.FromDateTime(dateTime.Value);

		// The following extension methods enable the injection of query hints.

		/// <summary>
		/// Emit SQL query hint(s) such as 'OPTIMIZE FOR UNKNOWN' or 'HASH GROUP'.
		/// </summary>
		public static IQueryable<TSource> WithQueryHints<TSource> (this IQueryable<TSource> source, params string[] sqlQueryHints)
		{
			if (source == null)
				throw Error.ArgumentNull("source");

			if (sqlQueryHints == null || sqlQueryHints.Length == 0 || source is ITable)   // Not supported directly on tables
				return source;

			return source.Provider.CreateQuery<TSource>(
				Expression.Call(
					null,
					GetMethodInfo(WithQueryHints, source, sqlQueryHints),
					new Expression[] { source.Expression, Expression.Constant(sqlQueryHints) }
					));
		}

		/// <summary>
		/// Emits the query hint 'OPTIMIZE FOR UNKNOWN', instructing SQL Server's optimizer not to rely on specific parameter values when sampling statistics for the query plan.
		/// </summary>
		public static IQueryable<TSource> WithoutParameterSniffing<TSource> (this IQueryable<TSource> source)
			=> WithQueryHints(source, "OPTIMIZE FOR UNKNOWN");

		private static MethodInfo GetMethodInfo<T1, T2> (Func<T1, T2> f, T1 unused1)
		{
			return f.Method;
		}

		private static MethodInfo GetMethodInfo<T1, T2, T3> (Func<T1, T2, T3> f, T1 unused1, T2 unused2)
		{
			return f.Method;
		}
	}
}
	#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

			if (sqlQueryHints == null || sqlQueryHints.Length == 0)
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

		/// <summary>
		/// Annotates the query with the given tag, which is emitted as a leading SQL comment. This makes the query
		/// easy to identify in SQL Profiler, Query Store and other database tooling. Multiple tags accumulate.
		/// </summary>
		public static IQueryable<TSource> TagWith<TSource> (this IQueryable<TSource> source, string tag)
		{
			if (source == null)
				throw Error.ArgumentNull("source");

			if (string.IsNullOrWhiteSpace(tag))
				return source;

			return source.Provider.CreateQuery<TSource>(
				Expression.Call(
					null,
					GetMethodInfo(TagWith, source, tag),
					new Expression[] { source.Expression, Expression.Constant(tag) }
					));
		}

		/// <summary>
		/// Annotates the query with a tag identifying the source file and line number from which it was called,
		/// emitted as a leading SQL comment. This makes the query easy to trace back to your code in SQL Profiler,
		/// Query Store and other database tooling. Note that this embeds the absolute source file path (as it was
		/// at compile time) into the query text.
		/// </summary>
		public static IQueryable<TSource> TagWithCallSite<TSource> (this IQueryable<TSource> source,
			[CallerFilePath] string filePath = null, [CallerMemberName] string memberName = null, [CallerLineNumber] int lineNumber = 0)
		{
			if (source == null)
				throw Error.ArgumentNull("source");

			if (string.IsNullOrWhiteSpace(filePath))
				return source;

			string s = "File: " + 
				(filePath.EndsWith(Path.DirectorySeparatorChar + "LINQPadQuery", StringComparison.OrdinalIgnoreCase) ? "Script" : filePath);
			
			if (!string.IsNullOrEmpty(memberName))
				s += ":" + memberName;

			if (lineNumber > 0)
				s += ":" + lineNumber;

            return TagWith(source, s);
		}

		// --- Set-based DML (ExecuteDelete / ExecuteUpdate) ---

		/// <summary>
		/// Deletes all rows matched by the query in a single DELETE statement, without loading entities or
		/// involving the change tracker. Executes immediately (independently of SubmitChanges) and returns the
		/// number of rows deleted. Note that already-tracked entities are not refreshed by this operation.
		/// </summary>
		public static int ExecuteDelete<TSource> (this IQueryable<TSource> source)
		{
			if (source == null)
				throw Error.ArgumentNull("source");

			return source.Provider.Execute<int>(
				Expression.Call(
					null,
					GetMethodInfo(ExecuteDelete, source),
					new Expression[] { source.Expression }
					));
		}

		/// <summary>
		/// Updates all rows matched by the query in a single UPDATE statement, without loading entities or
		/// involving the change tracker. Columns are assigned via a chain of SetProperty calls, e.g.
		/// <c>q.ExecuteUpdate(s => s.SetProperty(r => r.Price, r => r.Price * 1.1m).SetProperty(r => r.Flag, true))</c>.
		/// Executes immediately (independently of SubmitChanges) and returns the number of rows updated.
		/// Note that already-tracked entities are not refreshed by this operation.
		/// </summary>
		public static int ExecuteUpdate<TSource> (this IQueryable<TSource> source,
			Expression<Func<SetPropertyCalls<TSource>, SetPropertyCalls<TSource>>> setPropertyCalls)
		{
			if (source == null)
				throw Error.ArgumentNull("source");
			if (setPropertyCalls == null)
				throw Error.ArgumentNull("setPropertyCalls");

			return source.Provider.Execute<int>(
				Expression.Call(
					null,
					GetMethodInfo(ExecuteUpdate, source, setPropertyCalls),
					new Expression[] { source.Expression, Expression.Quote(setPropertyCalls) }
					));
		}

		private static MethodInfo GetMethodInfo<T1, T2> (Func<T1, T2> f, T1 unused1)
		{
			return f.Method;
		}

		private static MethodInfo GetMethodInfo<T1, T2, T3> (Func<T1, T2, T3> f, T1 unused1, T2 unused2)
		{
			return f.Method;
		}
	}

	/// <summary>
	/// Supports specifying the column assignments of <see cref="LinqToSqlExtensions.ExecuteUpdate{TSource}"/>.
	/// This type exists only to be used within that method's expression tree; its members cannot be invoked directly.
	/// </summary>
	public sealed class SetPropertyCalls<TSource>
	{
		private SetPropertyCalls () { }

		/// <summary>
		/// Assigns the column selected by <paramref name="propertyExpression"/> a value computed from the row.
		/// </summary>
		public SetPropertyCalls<TSource> SetProperty<TProperty> (Func<TSource, TProperty> propertyExpression, Func<TSource, TProperty> valueExpression)
			=> throw new InvalidOperationException("SetProperty can only be used within an ExecuteUpdate call.");

		/// <summary>
		/// Assigns the column selected by <paramref name="propertyExpression"/> a constant value.
		/// </summary>
		public SetPropertyCalls<TSource> SetProperty<TProperty> (Func<TSource, TProperty> propertyExpression, TProperty valueExpression)
			=> throw new InvalidOperationException("SetProperty can only be used within an ExecuteUpdate call.");
	}
}
	#endif

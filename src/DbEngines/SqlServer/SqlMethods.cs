using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.SqlClient
{
	/// <summary>
	/// Methods which are to be used inside Linq to Sql queries as alternatives to regular Linq methods. 
	/// </summary>
	public static class SqlMethods
	{
		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of year boundaries crossed between the dates.</returns>
		public static int DateDiffYear(DateTime startDate, DateTime endDate)
		{
			return endDate.Year - startDate.Year;
		}


		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of year boundaries crossed between the dates.</returns>
		public static int? DateDiffYear(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffYear(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of year boundaries crossed between the dates.</returns>
		public static int DateDiffYear(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffYear(startDate.UtcDateTime, endDate.UtcDateTime);
		}


		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of year boundaries crossed between the dates.</returns>
		public static int? DateDiffYear(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffYear(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of month boundaries crossed between the dates.</returns>
		public static int DateDiffMonth(DateTime startDate, DateTime endDate)
		{
			return 12 * (endDate.Year - startDate.Year) + endDate.Month - startDate.Month;
		}

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of month boundaries crossed between the dates.</returns>
		public static int? DateDiffMonth(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMonth(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of month boundaries crossed between the dates.</returns>
		public static int DateDiffMonth(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffMonth(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of month boundaries crossed between the dates.</returns>

		public static int? DateDiffMonth(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMonth(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of day boundaries crossed between the dates.</returns>
		public static int DateDiffDay(DateTime startDate, DateTime endDate)
		{
			TimeSpan diff = endDate.Date - startDate.Date;
			return diff.Days;
		}

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of day boundaries crossed between the dates.</returns>
		public static int? DateDiffDay(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffDay(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of day boundaries crossed between the dates.</returns>
		public static int DateDiffDay(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffDay(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of day boundaries crossed between the dates.</returns>
		public static int? DateDiffDay(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffDay(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of hour boundaries crossed between the dates.</returns>
		public static int DateDiffHour(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return DateDiffDay(startDate, endDate) * 24 + endDate.Hour - startDate.Hour;
			}
		}

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of hour boundaries crossed between the dates.</returns>
		public static int? DateDiffHour(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffHour(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of hour boundaries crossed between the dates.</returns>
		public static int DateDiffHour(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffHour(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of hour boundaries crossed between the dates.</returns>
		public static int? DateDiffHour(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffHour(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of minute boundaries crossed between the dates.</returns>
		public static int DateDiffMinute(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return DateDiffHour(startDate, endDate) * 60 + endDate.Minute - startDate.Minute;
			}
		}

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of minute boundaries crossed between the dates.</returns>
		public static int? DateDiffMinute(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMinute(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of minute boundaries crossed between the dates.</returns>
		public static int DateDiffMinute(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffMinute(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of minute boundaries crossed between the dates.</returns>

		public static int? DateDiffMinute(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMinute(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of second boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of second boundaries crossed between the dates.</returns>
		public static int DateDiffSecond(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return DateDiffMinute(startDate, endDate) * 60 + endDate.Second - startDate.Second;
			}
		}

		/// <summary>
		/// Counts the number of second boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of second boundaries crossed between the dates.</returns>
		public static int? DateDiffSecond(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffSecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of second boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of second boundaries crossed between the dates.</returns>
		public static int DateDiffSecond(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffSecond(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of second boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of second boundaries crossed between the dates.</returns>

		public static int? DateDiffSecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffSecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of millisecond boundaries crossed between the dates.</returns>
		public static int DateDiffMillisecond(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return DateDiffSecond(startDate, endDate) * 1000 + endDate.Millisecond - startDate.Millisecond;
			}
		}

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of millisecond boundaries crossed between the dates.</returns>
		public static int? DateDiffMillisecond(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMillisecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of millisecond boundaries crossed between the dates.</returns>
		public static int DateDiffMillisecond(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffMillisecond(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of millisecond boundaries crossed between the dates.</returns>

		public static int? DateDiffMillisecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMillisecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of microsecond boundaries crossed between the dates.</returns>
		public static int DateDiffMicrosecond(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return (int)(endDate.Ticks / 10 - startDate.Ticks / 10);
			}
		}

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of microsecond boundaries crossed between the dates.</returns>
		public static int? DateDiffMicrosecond(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMicrosecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of microsecond boundaries crossed between the dates.</returns>
		public static int DateDiffMicrosecond(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffMicrosecond(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of microsecond boundaries crossed between the dates.</returns>

		public static int? DateDiffMicrosecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffMicrosecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
		public static int DateDiffNanosecond(DateTime startDate, DateTime endDate)
		{
			checked
			{
				return (int)((endDate.Ticks - startDate.Ticks) * 100);
			}
		}

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
		public static int? DateDiffNanosecond(DateTime? startDate, DateTime? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffNanosecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
		public static int DateDiffNanosecond(DateTimeOffset startDate, DateTimeOffset endDate)
		{
			return DateDiffNanosecond(startDate.UtcDateTime, endDate.UtcDateTime);
		}

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startDate,endDate).
		/// </summary>
		/// <param name="startDate">Starting date for the calculation.</param>
		/// <param name="endDate">Ending date for the calculation.</param>
		/// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
		public static int? DateDiffNanosecond(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			if(startDate.HasValue && endDate.HasValue)
			{
				return DateDiffNanosecond(startDate.Value, endDate.Value);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		public static int DateDiffYear(DateOnly startDate, DateOnly endDate) => endDate.Year - startDate.Year;

		/// <summary>
		/// Counts the number of year boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(YEAR,startDate,endDate).
		/// </summary>
		public static int? DateDiffYear(DateOnly? startDate, DateOnly? endDate) =>
			startDate.HasValue && endDate.HasValue ? DateDiffYear(startDate.Value, endDate.Value) : null;

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		public static int DateDiffMonth(DateOnly startDate, DateOnly endDate) =>
			(endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

		/// <summary>
		/// Counts the number of month boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(MONTH,startDate,endDate).
		/// </summary>
		public static int? DateDiffMonth(DateOnly? startDate, DateOnly? endDate) =>
			startDate.HasValue && endDate.HasValue ? DateDiffMonth(startDate.Value, endDate.Value) : null;

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		public static int DateDiffDay(DateOnly startDate, DateOnly endDate) => endDate.DayNumber - startDate.DayNumber;

		/// <summary>
		/// Counts the number of day boundaries crossed between the startDate and endDate.
		/// Corresponds to SQL Server's DATEDIFF(DAY,startDate,endDate).
		/// </summary>
		public static int? DateDiffDay(DateOnly? startDate, DateOnly? endDate) =>
			startDate.HasValue && endDate.HasValue ? DateDiffDay(startDate.Value, endDate.Value) : null;

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startTime,endTime).
		/// </summary>
		public static int DateDiffHour(TimeOnly startTime, TimeOnly endTime) => endTime.Hour - startTime.Hour;

		/// <summary>
		/// Counts the number of hour boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(HOUR,startTime,endTime).
		/// </summary>
		public static int? DateDiffHour(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffHour(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startTime,endTime).
		/// </summary>
		public static int DateDiffMinute(TimeOnly startTime, TimeOnly endTime) =>
			DateDiffHour(startTime, endTime) * 60 + endTime.Minute - startTime.Minute;

		/// <summary>
		/// Counts the number of minute boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MINUTE,startTime,endTime).
		/// </summary>
		public static int? DateDiffMinute(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffMinute(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// Counts the number of second boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startTime,endTime).
		/// </summary>
		public static int DateDiffSecond(TimeOnly startTime, TimeOnly endTime) =>
			DateDiffMinute(startTime, endTime) * 60 + endTime.Second - startTime.Second;

		/// <summary>
		/// Counts the number of second boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(SECOND,startTime,endTime).
		/// </summary>
		public static int? DateDiffSecond(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffSecond(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startTime,endTime).
		/// </summary>
		public static int DateDiffMillisecond(TimeOnly startTime, TimeOnly endTime) =>
			(int)(endTime.Ticks / TimeSpan.TicksPerMillisecond - startTime.Ticks / TimeSpan.TicksPerMillisecond);

		/// <summary>
		/// Counts the number of millisecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MILLISECOND,startTime,endTime).
		/// </summary>
		public static int? DateDiffMillisecond(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffMillisecond(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startTime,endTime).
		/// </summary>
		public static int DateDiffMicrosecond(TimeOnly startTime, TimeOnly endTime)
		{
			checked
			{
				return (int)(endTime.Ticks / 10 - startTime.Ticks / 10);
			}
		}

		/// <summary>
		/// Counts the number of microsecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(MICROSECOND,startTime,endTime).
		/// </summary>
		public static int? DateDiffMicrosecond(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffMicrosecond(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startTime,endTime).
		/// </summary>
		public static int DateDiffNanosecond(TimeOnly startTime, TimeOnly endTime)
		{
			checked
			{
				return (int)((endTime.Ticks - startTime.Ticks) * 100);
			}
		}

		/// <summary>
		/// Counts the number of nanosecond boundaries crossed between the startTime and endTime.
		/// Corresponds to SQL Server's DATEDIFF(NANOSECOND,startTime,endTime).
		/// </summary>
		public static int? DateDiffNanosecond(TimeOnly? startTime, TimeOnly? endTime) =>
			startTime.HasValue && endTime.HasValue ? DateDiffNanosecond(startTime.Value, endTime.Value) : null;

		/// <summary>
		/// This function is translated to Sql Server's LIKE function.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="match_expression">The string that is to be matched.</param>
		/// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
		/// <returns>true if there is a match.</returns>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pattern", Justification = "[....]: Method is a placeholder for a server-side method.")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "matchExpression", Justification = "[....]: Method is a placeholder for a server-side method.")]
		public static bool Like(string matchExpression, string pattern)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to Sql Server's LIKE function.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="match_expression">The string that is to be matched.</param>
		/// <param name="pattern">The pattern which may involve wildcards %,_,[,],^.</param>
		/// <param name="escape_character">The escape character to use in front of %,_,[,],^ if they are not used as wildcards.</param>
		/// <returns>true if there is a match.</returns>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "pattern", Justification = "[....]: Method is a placeholder for a server-side method.")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "matchExpression", Justification = "[....]: Method is a placeholder for a server-side method.")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "escapeCharacter", Justification = "[....]: Method is a placeholder for a server-side method.")]
		public static bool Like(string matchExpression, string pattern, char escapeCharacter)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_VALUE function, which extracts a scalar value
		/// from a JSON document. Works with both json (SQL Server 2025+) and nvarchar columns.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="path">A JSON path identifying the scalar to extract, e.g. "$.name".</param>
		/// <returns>The extracted scalar as a string, or null if the path isn't found.</returns>
		public static string JsonValue(string json, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_QUERY function, which extracts an object or
		/// array from a JSON document. Works with both json (SQL Server 2025+) and nvarchar columns.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="path">A JSON path identifying the object or array to extract, e.g. "$.tags".</param>
		/// <returns>The extracted JSON fragment as a string, or null if the path isn't found.</returns>
		public static string JsonQuery(string json, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's ISJSON function (as ISJSON(json) = 1).
		/// A null input yields NULL on the server, which is treated as false in predicates.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The string to test.</param>
		/// <returns>true if the string contains valid JSON.</returns>
		public static bool IsJson(string json)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_PATH_EXISTS function (as JSON_PATH_EXISTS(json, path) = 1).
		/// Requires SQL Server 2022 or later. A null input yields NULL on the server, which is treated as false in predicates.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="path">The JSON path to test for, e.g. "$.address.city".</param>
		/// <returns>true if the document contains the path.</returns>
		public static bool JsonPathExists(string json, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_MODIFY function, which returns a copy of the
		/// document with the value at the given path replaced. Note that in a LINQ query this composes into
		/// projections and predicates; to persist a change, assign the result to the entity's property.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="path">A JSON path identifying the value to modify, e.g. "$.name".</param>
		/// <param name="newValue">The new value.</param>
		/// <returns>The modified document.</returns>
		public static string JsonModify(string json, string path, string newValue)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_CONTAINS function (in preview as of SQL Server 2025),
		/// which tests whether a JSON document contains the given SQL scalar value at the specified path.
		/// The comparison uses the SQL type of the value, so the number 2 and the string "2" are distinct.
		/// If the path targets an array, use a wildcard, e.g. "$.tags[*]". A null document yields NULL on
		/// the server, which is treated as false in predicates.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="value">The string value to search for.</param>
		/// <param name="path">A JSON path identifying where to search, e.g. "$.name".</param>
		/// <returns>true if the document contains the value at the path.</returns>
		public static bool JsonContains(string json, string value, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_CONTAINS function (in preview as of SQL Server 2025).
		/// See the other overloads for details.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="json">The JSON document.</param>
		/// <param name="value">The string value or pattern to search for.</param>
		/// <param name="path">A JSON path identifying where to search, e.g. "$.name".</param>
		/// <param name="searchMode">0 for equality semantics (the default); 1 for LIKE pattern semantics.</param>
		/// <returns>true if the document contains a matching value at the path.</returns>
		public static bool JsonContains(string json, string value, string path, int searchMode)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_CONTAINS function (in preview as of SQL Server 2025).
		/// See the string overload for details.
		/// It cannot be used on the client.
		/// </summary>
		public static bool JsonContains(string json, long value, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_CONTAINS function (in preview as of SQL Server 2025).
		/// See the string overload for details. Note there's no float/double overload because the server
		/// rejects approximate-numeric search values; use the decimal overload for fractional numbers.
		/// It cannot be used on the client.
		/// </summary>
		public static bool JsonContains(string json, decimal value, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's JSON_CONTAINS function (in preview as of SQL Server 2025).
		/// See the string overload for details.
		/// It cannot be used on the client.
		/// </summary>
		public static bool JsonContains(string json, bool value, string path)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to SQL Server's VECTOR_DISTANCE function (SQL Server 2025+),
		/// which computes the distance between two vectors. At least one operand must be a mapped
		/// vector column (its declared dimension types the other operand). A null vector yields NULL,
		/// which is treated as false in predicates.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="distanceMetric">The distance metric: "cosine", "euclidean" or "dot".</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The distance between the vectors.</returns>
		public static double VectorDistance(this float[] vector1, string distanceMetric, float[] vector2)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to Sql Server's DATALENGTH function.  It differs
		/// from LEN in that it includes trailing spaces and will count UNICODE characters
		/// per byte.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="value">The string to take the length of.</param>
		/// <returns>length of the string</returns>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "[....]: Method is a placeholder for a server-side method.")]
		internal static int RawLength(string value)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to Sql Server's DATALENGTH function.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="value">The byte array to take the length of.</param>
		/// <returns>length of the array</returns>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "[....]: Method is a placeholder for a server-side method.")]
		internal static int RawLength(byte[] value)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}

		/// <summary>
		/// This function is translated to Sql Server's DATALENGTH function.
		/// It cannot be used on the client.
		/// </summary>
		/// <param name="value">The Binary value to take the length of.</param>
		/// <returns>length of the Binary</returns>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value", Justification = "[....]: Method is a placeholder for a server-side method.")]
		internal static int RawLength(Binary value)
		{
			throw Error.SqlMethodOnlyForSql(MethodInfo.GetCurrentMethod());
		}
	}
}

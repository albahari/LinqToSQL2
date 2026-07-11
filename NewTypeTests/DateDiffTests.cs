using Xunit;
using System;
using System.Linq;
using System.Data.Linq.SqlClient;

namespace NewTypeTests
{
	public class DateDiffTests : IClassFixture<DatabaseFixture>
	{
		// The fixture row has DateOnly = 2020-01-02, TimeOnly = 10:11:12.123
		static readonly DateOnly TestDate = new (2020, 1, 2);
		static readonly TimeOnly TestTime = new (10, 11, 12, 123);

		DatabaseFixture _fixture;
		TypedDataContext Data => _fixture.Data;

		public DateDiffTests (DatabaseFixture fixture) => _fixture = fixture;

		[Fact]
		public void DateOnlyDiffsTranslate ()
		{
			Assert.Equal (1, Data.DateTimeTests.Count (d => SqlMethods.DateDiffDay (d.DateOnly!.Value, TestDate.AddDays (30)) == 30));
			Assert.Equal (1, Data.DateTimeTests.Count (d => SqlMethods.DateDiffMonth (d.DateOnly!.Value, TestDate.AddMonths (2)) == 2));
			Assert.Equal (1, Data.DateTimeTests.Count (d => SqlMethods.DateDiffYear (d.DateOnly!.Value, TestDate.AddYears (3)) == 3));
		}

		[Fact]
		public void NullableDateOnlyDiffsTranslate () =>
			Assert.Equal (1, Data.DateTimeTests.Count (d => SqlMethods.DateDiffDay (d.DateOnly, TestDate.AddDays (7)) == 7));

		[Fact]
		public void TimeOnlyDiffsTranslate ()
		{
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffHour (t.TimeOnly!.Value, TestTime.AddHours (3)) == 3));
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffMinute (t.TimeOnly!.Value, TestTime.AddMinutes (90)) == 90));
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffSecond (t.TimeOnly!.Value, TestTime.Add (TimeSpan.FromSeconds (45))) == 45));
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffMillisecond (t.TimeOnly!.Value, TestTime.Add (TimeSpan.FromMilliseconds (250))) == 250));
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffMicrosecond (t.TimeOnly!.Value, TestTime.Add (TimeSpan.FromMilliseconds (1))) == 1000));
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffNanosecond (t.TimeOnly!.Value, TestTime.Add (TimeSpan.FromMilliseconds (1))) == 1000000));
		}

		[Fact]
		public void NullableTimeOnlyDiffsTranslate () =>
			Assert.Equal (1, Data.DateTimeTests.Count (t => SqlMethods.DateDiffMinute (t.TimeOnly, TestTime.AddMinutes (5)) == 5));

		[Fact]
		public void ClientSideSemanticsMatchBoundaryCounting ()
		{
			// DATEDIFF counts boundary crossings, not elapsed time
			Assert.Equal (1, SqlMethods.DateDiffYear (new DateOnly (2020, 12, 31), new DateOnly (2021, 1, 1)));
			Assert.Equal (1, SqlMethods.DateDiffMonth (new DateOnly (2020, 12, 31), new DateOnly (2021, 1, 1)));
			Assert.Equal (60, SqlMethods.DateDiffDay (new DateOnly (2020, 1, 1), new DateOnly (2020, 3, 1)));
			Assert.Equal (-60, SqlMethods.DateDiffDay (new DateOnly (2020, 3, 1), new DateOnly (2020, 1, 1)));

			Assert.Equal (1, SqlMethods.DateDiffHour (new TimeOnly (10, 59, 59), new TimeOnly (11, 0, 0)));
			Assert.Equal (1, SqlMethods.DateDiffMinute (new TimeOnly (10, 59, 59), new TimeOnly (11, 0, 0)));
			Assert.Equal (1000000, SqlMethods.DateDiffNanosecond (new TimeOnly (0, 0, 0), new TimeOnly (0, 0, 0, 1)));

			// Sub-unit tick fractions still count: crossing from 0.9999ms to 1.0ms is one millisecond
			// boundary (and analogously for microseconds), even though the elapsed time is a single tick.
			Assert.Equal (1, SqlMethods.DateDiffMillisecond (new TimeOnly (9_999), new TimeOnly (10_000)));
			Assert.Equal (0, SqlMethods.DateDiffMillisecond (new TimeOnly (10_000), new TimeOnly (19_999)));
			Assert.Equal (1, SqlMethods.DateDiffMicrosecond (new TimeOnly (9), new TimeOnly (10)));
			Assert.Equal (0, SqlMethods.DateDiffMicrosecond (new TimeOnly (10), new TimeOnly (19)));
			Assert.Equal (-1, SqlMethods.DateDiffMicrosecond (new TimeOnly (10), new TimeOnly (9)));
			Assert.Equal (1, SqlMethods.DateDiffMicrosecond (new DateTime (9), new DateTime (10)));

			Assert.Null (SqlMethods.DateDiffDay ((DateOnly?) null, TestDate));
			Assert.Null (SqlMethods.DateDiffSecond (TestTime, (TimeOnly?) null));
		}
	}
}

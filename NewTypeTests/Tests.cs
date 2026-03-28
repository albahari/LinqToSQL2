using Xunit;
using System;
using System.Linq;
using System.Data.Linq;

[assembly: CollectionBehavior (DisableTestParallelization = true)]

namespace NewTypeTests
{
    public class Tests : IClassFixture<DatabaseFixture>
    {
        DatabaseFixture _fixture;
        TypedDataContext Data => _fixture.Data;

        static readonly DateOnly TestDate = new DateOnly (2020, 1, 2);
        static readonly TimeOnly TestTime = new TimeOnly (10, 11, 12, 123);

        public Tests (DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void BasicSelectWorks ()
        {
            var row = Data.DateTimeTests.Single (d => d.ID == 1); 
            Assert.Equal (1, row.ID);
            Assert.Equal (TestDate, row.DateOnly);
        }

        [Fact]
        public void CanAddRemove ()
        {
            var newRow = new DateTimeTest { ID = 2, DateOnly = new DateOnly (2022, 4, 5), TimeOnly = new TimeOnly (2, 3, 4) };
            Data.DateTimeTests.InsertOnSubmit (newRow);
            Data.SubmitChanges ();
            var newData = new TypedDataContext ();
            var fetched = newData.DateTimeTests.Single (x => x.ID == 2);
            Assert.Equal (newRow.DateOnly, fetched.DateOnly);
            Assert.Equal (newRow.TimeOnly, fetched.TimeOnly);
            newData.DateTimeTests.DeleteOnSubmit (fetched);
            newData.SubmitChanges ();
        }

        [Fact]
        public void CanUseDate () => Assert.Equal (1, Data.DateTimeTests.Count (d => d.DateOnly == TestDate));
        
        [Fact]
        public void CanUseTime () => Assert.Equal (1, Data.DateTimeTests.Count (d => d.TimeOnly == TestTime));

        [Fact]
        public void CanAddToDate()
        {
            var yesterday = TestDate.AddDays (-1);
            Assert.Equal (1, Data.DateTimeTests.Count (d => d.DateOnly!.Value.AddDays (-1) == yesterday));
        }

        [Fact]
        public void CanAddToTime ()
        {
            var lastHour = TestTime.AddHours (-1);
            Assert.Equal (1, Data.DateTimeTests.Count (d => d.TimeOnly!.Value.AddHours (-1) == lastHour));
        }

        [Fact]
        public void CanAddTimeSpanToTime ()
        {
            var lastHour = TestTime.AddHours (-1);
            Assert.Equal (1, Data.DateTimeTests.Count (d => d.TimeOnly.Value.Add (TimeSpan.FromHours (-1)) == lastHour));
        }

        [Fact]
        public void CanSelectDatePart()
        {
            Assert.Equal (1, Data.DateTimeTests.Count (d => 
                d.DateOnly!.Value.Year == TestDate.Year &&
                d.DateOnly.Value.Month == TestDate.Month &&
                d.DateOnly.Value.Day == TestDate.Day));
        }

        [Fact]
        public void CanSelectTimePart ()
        {
            Assert.Equal (1, Data.DateTimeTests.Count (d => 
                d.TimeOnly!.Value.Hour == TestTime.Hour &&
                d.TimeOnly.Value.Minute == TestTime.Minute &&
                d.TimeOnly.Value.Second == TestTime.Second));
        }

        [Fact]
        public void CanConstructDate()
        {
            var matches = Data.DateTimeTests
                .Where (d => d.Year != null && d.Month != null && d.Day != null)
                .Select (d => new DateOnly (d.Year!.Value, d.Month!.Value, d.Day!.Value))
                .ToArray ();
            Assert.Equal (1, matches.Count (m => m == TestDate));
        }

        [Fact]
        public void CanConstructTime ()
        {
            var matches = Data.DateTimeTests
                .Where (d => d.Hour != null && d.Minute != null && d.Second != null && d.Millisecond != null)
                .Select (d => new TimeOnly (d.Hour!.Value, d.Minute!.Value, d.Second!.Value, d.Millisecond!.Value))
                .ToArray ();
            Assert.Equal (1, matches.Count (m => m == TestTime));
        }

        [Fact]
        public void CanConstructTimeWithMS ()
        {
            var matches = Data.DateTimeTests
                .Where (d => d.Hour != null && d.Minute != null && d.Second != null && d.Millisecond != null)
                .Select (d => new TimeOnly (d.Hour!.Value, d.Minute!.Value, d.Second!.Value, d.Millisecond!.Value))
                .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanCompareDateTimeWithDateOnly()
        {
            var matches = Data.DateTimeTests
               .Where (d => d.DateAndTime!.Value.DateOnly() == d.DateOnly)
               .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanCompareNullableDateTimeWithDateOnly ()
        {
            var matches = Data.DateTimeTests
               .Where (d => d.DateAndTime.DateOnly () == d.DateOnly)
               .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanCompareDateTimeWithTimeOnly ()
        {
            var matches = Data.DateTimeTests
               .Where (d => d.DateAndTime!.Value.TimeOnly () == d.TimeOnly)
               .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanCompareNullableDateTimeWithTimeOnly ()
        {
            var matches = Data.DateTimeTests
               .Where (d => d.DateAndTime.TimeOnly () == d.TimeOnly)
               .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanCompareDateTimeWithDateOnlyUsingStaticMethod ()
        {
            var matches = Data.DateTimeTests
               .Where (d => DateOnly.FromDateTime (d.DateAndTime!.Value) == d.DateOnly)
               .ToArray ();
            Assert.Single (matches);
        }

        [Fact]
        public void CanQueryDayOfYear()
        {
            var matches = Data.DateTimeTests
               .Where (d => d.DateOnly!.Value.DayOfYear == TestDate.DayOfYear)
               .ToArray ();
            Assert.Single (matches);
        }

		[Fact]
		public void CanUseSpanContains ()
		{
            var keys = new[] { 1, 1000 };
			var matches = Data.DateTimeTests
			   .Where(d => keys.Contains (d.ID))
			   .ToArray();
			Assert.Single(matches);
		}

        [Fact]
        public void CanUseSpanContainsWithDoW()
        {
            var days = new[] { DayOfWeek.Thursday, DayOfWeek.Friday};
            var matches = Data.DateTimeTests
               .Where(d => days.Contains(d.DateAndTime.Value.DayOfWeek))
               .ToArray();
            Assert.Single(matches);
        }

    }
}
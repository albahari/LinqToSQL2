using System;
using System.Collections.Generic;
using System.Data.Linq.DbEngines.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTypeTests
{
    public class DatabaseFixture : IDisposable
    {
        public readonly TypedDataContext Data;

        public DatabaseFixture ()
        {
			SqlLibrary.PreferMicrosoftDataClient = true;
			Data = new TypedDataContext();

			Data.ExecuteCommand (@"drop table if exists DateTimeTest
create table DateTimeTest (ID int not null primary key, DateOnly Date, TimeOnly Time, DateAndTime datetime2, Year int, Month int, Day int, Hour int, Minute int, Second int, Millisecond int)
insert DateTimeTest 
(ID, DateOnly, TimeOnly, DateAndTime, Year, Month, Day, Hour, Minute, Second, Millisecond) values 
(1, '2020-1-2', '10:11:12.123', '2020-1-2 10:11:12.123', 2020, 1, 2, 10, 11, 12, 123)");
        }

        public void Dispose () => Data.Dispose ();
    }
}

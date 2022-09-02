using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewTypeTests
{
	public class TypedDataContext : DataContext
	{
		private static MappingSource _mapping;

		public Table<DateTimeTest> DateTimeTests => GetTable<DateTimeTest> ();		

		static TypedDataContext ()
		{
			_mapping = new AttributeMappingSource ();
		}

		public TypedDataContext ()
			: base ("Data Source=.;Integrated Security=SSPI;Initial Catalog=L2SNewTypeTests;TrustServerCertificate=true", _mapping)
		{
		}
	}


	[Table (Name = "[DateTimeTest]")]
	public class DateTimeTest
	{
		[Column (IsPrimaryKey = true, DbType = "Int NOT NULL", UpdateCheck = UpdateCheck.Never)]
		public int ID;

		[Column] 
		public DateOnly? DateOnly;
		
		[Column]
		public TimeOnly? TimeOnly;
		
		[Column]
		public DateTime? DateAndTime;

		[Column]
		public int? Year, Month, Day, Hour, Minute, Second, Millisecond;
	}

	[Table (Name = "[DateTimeTest]")]
	public class DateTimeTest2
	{
		[Column (IsPrimaryKey = true, DbType = "Int NOT NULL", UpdateCheck = UpdateCheck.Never)]
		public int ID;

		[Column (DbType = "Date", UpdateCheck = UpdateCheck.Never)]
		public DateOnly? DateOnly;

		[Column (DbType = "Time", UpdateCheck = UpdateCheck.Never)]
		public TimeOnly? TimeOnly;

		[Column (DbType = "DateTime", UpdateCheck = UpdateCheck.Never)]
		public DateTime? DateAndTime;

		[Column]
		public int? Year, Month, Day, Hour, Minute, Second, Millisecond;
	}

}

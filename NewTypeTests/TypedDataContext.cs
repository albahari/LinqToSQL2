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
		public Table<JsonTest> JsonTests => GetTable<JsonTest> ();
		public Table<VectorTest> VectorTests => GetTable<VectorTest> ();
		public Table<VectorTest2> VectorTest2s => GetTable<VectorTest2> ();

		static TypedDataContext ()
		{
			_mapping = new AttributeMappingSource ();
		}

		public TypedDataContext ()
			: base (TestConfig.ConnectionString, _mapping)
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

	[Table (Name = "[JsonTest]")]
	public class JsonTest
	{
		[Column (IsPrimaryKey = true, DbType = "Int NOT NULL", UpdateCheck = UpdateCheck.Never)]
		public int ID;

		[Column (DbType = "Json", UpdateCheck = UpdateCheck.Never)]
		public string? Doc;
	}

	[Table (Name = "[VectorTest]")]
	public class VectorTest2   // maps Vec without a dimension, so VectorDistance must infer it from the client operand
	{
		[Column (IsPrimaryKey = true, DbType = "Int NOT NULL", UpdateCheck = UpdateCheck.Never)]
		public int ID;

		[Column (DbType = "Vector", UpdateCheck = UpdateCheck.Never)]
		public float[]? Vec;
	}

	[Table (Name = "[VectorTest]")]
	public class VectorTest
	{
		[Column (IsPrimaryKey = true, DbType = "Int NOT NULL", UpdateCheck = UpdateCheck.Never)]
		public int ID;

		[Column (DbType = "Vector(3)", UpdateCheck = UpdateCheck.Never)]
		public float[]? Vec;
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

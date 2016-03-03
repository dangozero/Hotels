using System;
using SQLite;

namespace Hotels
{
	public class Booking
	{
		[PrimaryKey, AutoIncrement]
		public int Id {
			get;
			set;
		}

		[Indexed]
		public string Date {
			get;
			set;
		}

		[Indexed]
		public string Time {
			get;
			set;
		}
	}
}


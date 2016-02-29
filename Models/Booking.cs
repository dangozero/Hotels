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
		public int HotelId {
			get;
			set;
		}

		[Indexed]
		public DateTime Date {
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


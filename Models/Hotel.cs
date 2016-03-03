using System;
using SQLite;

namespace Hotels
{
	public class Hotel
	{
		[PrimaryKey, AutoIncrement]
		public int Id {
			get;
			set;
		}

		public string Title {
			get;
			set;
		}

		public string Anons {
			get;
			set;
		}

		public string Address {
			get;
			set;
		}

		public string Phone {
			get;
			set;
		}

		public int Price {
			get;
			set;
		}

		public int Review {
			get;
			set;
		}

		public int Rating {
			get;
			set;
		}
	}
}


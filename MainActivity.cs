using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;

namespace Hotels
{
	[Activity (Label = "@string/booking_title", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		private List<string> mTimes = new List<string>() {
			"09:00", "09:20", "09:40",
			"10:00", "10:20", "10:40", 
			"11:00", "11:20", "11:40", 
			"12:00", "12:20", "12:40", 
			"13:00", "13:20", "13:40", 
			"14:00", "14:20", "14:40", 
			"15:00", "15:20", "15:40", 
			"16:00", "16:20", "16:40", 
			"17:00", "17:20", "17:40", 
			"18:00", "18:20", "18:40", 
			"19:00", "19:20", "19:40", 
			"20:00", "20:20", "20:40", 
			"21:00", "21:20", "21:40", 
			"22:00", "22:20", "22:40", 
			"23:00", "23:20", "23:40", 
		};


		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			Initialize (savedInstanceState);

			SetContentView (Resource.Layout.Main);
		}

		void Initialize(Bundle savedInstanceState)
		{
			bool _initialized = Prefs.Current.GetValueOrDefault("Hotel-Demo-Intialized", false);
			if (!_initialized) {
				DatabaseHelper.Connection.CreateTableAsync<Hotel> ();
				DatabaseHelper.Connection.CreateTableAsync<Booking> ();


				DatabaseHelper.Connection.InsertAsync (new Hotel {
					Title = "Отель классный",
					Announce = "Самый лучший отель",
					Address = "г. Краснодар, ул. Красная",
					Phone = "8 800 800 80 80",
					Price = 5000,
					Rating = 3,
					Review = 12
				});

				Random r = new Random ();
				for (int i = 0; i < 10; i++) {
					DateTime randomDate = new DateTime (DateTime.Now.Year, DateTime.Now.Month, r.Next (1, 20));
					DatabaseHelper.Connection.InsertAsync (new Booking {
						HotelId = 1,
						Date = randomDate,
						Time = mTimes[r.Next(0, mTimes.Count-1)]
					});
				}

				Prefs.Current.AddOrUpdateValue ("Hotel-Demo-Intialized", true);
			}
		}
	}
}



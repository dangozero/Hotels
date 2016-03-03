using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System;

namespace Hotels
{
	[Activity (Label = "Hotels", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		private List<string> mTimes = new List<string> ();
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			ActionBar.Title = GetString (Resource.String.booking_page_title);

			InitialDemo ();

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			TabHost tabs = FindViewById<TabHost>(Android.Resource.Id.TabHost);

			tabs.Setup ();

			var booking = tabs.NewTabSpec ("bookingTag");
			booking.SetContent (Resource.Id.bookingTab);
			booking.SetIndicator(LayoutInflater.Inflate(Resource.Layout.BookingTab, null));
			tabs.AddTab (booking);

			var about = tabs.NewTabSpec ("aboutTag");
			about.SetContent (Resource.Id.aboutTab);
			about.SetIndicator (LayoutInflater.Inflate (Resource.Layout.AboutTab, null));
			tabs.AddTab (about);

			var review = tabs.NewTabSpec ("reviewTag");
			review.SetContent (Resource.Id.reviewTab);
			review.SetIndicator (LayoutInflater.Inflate (Resource.Layout.ReviewTab, null));
			tabs.AddTab (review);

			var map = tabs.NewTabSpec ("mapTag");
			map.SetContent (Resource.Id.mapTab);
			map.SetIndicator (LayoutInflater.Inflate (Resource.Layout.MapTab, null));
			tabs.AddTab (map);

			var menu = tabs.NewTabSpec ("menuTag");
			menu.SetContent (Resource.Id.menuTab);
			menu.SetIndicator (LayoutInflater.Inflate (Resource.Layout.MenuTab, null));
			tabs.AddTab (menu);


			tabs.CurrentTab = 0;
		}

		void InitialDemo()
		{
			DatabaseHelper.Connection.CreateTableAsync<Hotel> ();
			DatabaseHelper.Connection.CreateTableAsync<Booking> ();

			DatabaseHelper.Connection.ExecuteScalarAsync<int> ("SELECT COUNT(*) FROM Hotel").ContinueWith (te => {
				if( te.Result == 0 ) {
					DatabaseHelper.Connection.InsertAsync (new Hotel {
						Title = "Отель классный",
						Anons = "Самый лучший отель",
						Address = "г. Краснодар, ул. Красная",
						Phone = "8 800 800 80 80",
						Price = 5000,
						Rating = 3,
						Review = 15,
					});
				}
			}).Wait();

			DatabaseHelper.Connection.ExecuteScalarAsync<int> ("SELECT COUNT(*) FROM Booking").ContinueWith (te => {
				if( te.Result == 0 ) {
					DateTime it_tim = new DateTime (1970, 1, 1, 9, 0, 0);
					for (int i = 0; i < 45; i++) {
						mTimes.Add (it_tim.ToString ("HH:mm"));
						it_tim = it_tim.AddMinutes (20);
					}

					var random = new System.Random();
					for(int month = 1; month < 10; month++) {

						var schedules = random.Next(1, 10);
						var days = random.Next(5, 20);
						for(int k = 1; k < days; k++) {
							for(int j = 0; j < schedules; j++) {
								DatabaseHelper.Connection.InsertAsync(new Booking {
									Date = new DateTime(DateTime.Now.Year, month, k).ToString("yyyy-MM-dd"),
									Time = mTimes[random.Next(0, 44)]
								});
							}
						}
					}
				}
			}).Wait();
		}
	}
}



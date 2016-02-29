
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Graphics;


namespace Hotels
{
	public class CalendarFragment : Fragment
	{
		private Button mButton;
		private RecyclerView mListView;
		private GridView mGridView;
		private Button mBooking;

		private int mMonth;
		private int mYear;
		private DateTime mNow;

		private string[] mMonthNames;
		private string[] mDaysOfWeekNames;

		private List<string> mTimeItems = new List<string>() {
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

		private List<string> mDisabled;

		private DateTime mCurrentDate;
		private string mCurrentTime;
		private int mHotelId = 1;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			mNow = DateTime.Now;

			mYear = mNow.Year;
			Month = mNow.Month;

			mMonthNames = Activity.Resources.GetStringArray(Resource.Array.monthNamesShort);
			mDaysOfWeekNames = Activity.Resources.GetStringArray (Resource.Array.dayNamesMin);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.CalendarFragment, container, false);

			mBooking = view.FindViewById<Button> (Resource.Id.btnBooking);
			mBooking.Enabled = false;
			mBooking.Click += MBooking_Click;

			mButton = view.FindViewById<Button> (Resource.Id.btnDaysOrMonth);
			mButton.Text = mMonthNames [mMonth - 1];

			mListView = view.FindViewById<RecyclerView> (Resource.Id.daysOfMonthList);
			mListView.SetLayoutManager (new LinearLayoutManager (Activity, LinearLayoutManager.Horizontal, false));

			mGridView = view.FindViewById<GridView> (Resource.Id.timesOfDayList);


			CalendarAdapter adapter = new CalendarAdapter (this, mListView);
			adapter.Items = mItems;
			adapter.ItemClick += OnItemClick;
			mListView.SetAdapter (adapter);


			mGridView.Selected = true;
			mGridView.ItemClick += OnTimeItemClick;

			CalendarTimeAdapter time_adapter = new CalendarTimeAdapter (this, mGridView);
			time_adapter.Items = mTimeItems;
			mGridView.Adapter = time_adapter;

			return view;
		}

		void MBooking_Click (object sender, EventArgs e)
		{
			var adapter = mGridView.Adapter as CalendarTimeAdapter;
			adapter.SelectedIndex = -1;

			int time = DateTime.Now.ToUnixTimestamp ();
			int expires = Prefs.Current.GetValueOrDefault ("Hotel-Booking-Expires", 0);
			if ( expires < time ) {
				Prefs.Current.AddOrUpdateValue ("Hotel-Booking-Expires", time + 86400);
				DatabaseHelper.Connection.InsertAsync (new Booking {
					HotelId = mHotelId,
					Date = mCurrentDate,
					Time = mCurrentTime
				}).ContinueWith ((t) => {
					Activity.RunOnUiThread (() => {
						mBooking.Enabled = false;
						mDisabled.Add (mCurrentTime);
						adapter.NotifyDataSetChanged ();

						Toast.MakeText (Activity, string.Format (Activity.GetString (Resource.String.booking_done), mCurrentDate.ToString ("dd.MM.yyyy"), mCurrentTime), ToastLength.Long).Show ();
					});
				});
			} else {
				Toast.MakeText (Activity, string.Format (Activity.GetString (Resource.String.booking_once_per_day), mCurrentDate.ToString ("dd.MM.yyyy"), mCurrentTime), ToastLength.Long).Show ();
			}
		}

		void OnTimeItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			var adapter = mGridView.Adapter as CalendarTimeAdapter;
			var cl_adapter = mListView.GetAdapter() as CalendarAdapter;
			var holder = e.View.Tag as CalendarTimeAdapter.ViewHolder;
			if (holder.enabled) {
				adapter.SelectedIndex = e.Position;
				mCurrentTime = mTimeItems [e.Position];
				mBooking.Enabled = cl_adapter.SelectedIndex >= 0;
			}
			adapter.NotifyDataSetChanged ();
		}

		async void OnItemClick (object sender, int position)
		{
			var scheduleAdapter = mGridView.Adapter as CalendarTimeAdapter;
			mCurrentDate = mItems [position];
			mCurrentTime = string.Empty;
			scheduleAdapter.SelectedIndex = -1;
			mBooking.Enabled = false;

			var result = await DatabaseHelper
				.Connection
				.QueryAsync<Booking> ("SELECT * FROM Booking WHERE HotelId=? AND DATE(\"Date\")=?", mHotelId, mCurrentDate.ToString ("yyyy-MM-dd"));
			mDisabled = new List<string> ();
			foreach (Booking item in result) {
				mDisabled.Add (item.Time);
			}
			scheduleAdapter.NotifyDataSetChanged();

			var calendarAdapter = mListView.GetAdapter () as CalendarAdapter;
			calendarAdapter.SelectedIndex = position;
			calendarAdapter.NotifyDataSetChanged ();
		}

		public DateTime CurrentDate {
			get {
				return mCurrentDate;
			}
		}

		public List<string> Disabled {
			get {
				return mDisabled;
			}
		}

		private List<DateTime> mItems = new List<DateTime> ();
		public void ReloadData()
		{
			int days = DateTime.DaysInMonth (mYear, mMonth);
			DateTime beginDate = new DateTime (mYear, mMonth, 1);

			for (int i = 0; i < days; i++) {
				mItems.Add (beginDate.AddDays (i));
			}
		}

		public int Month {
			get { return mMonth; }
			set {
				if (mMonth != value) {
					mMonth = value;
					ReloadData ();
				}
			}
		}

		public string[] DaysOfWeekNames {
			get{
				return mDaysOfWeekNames;
			}
		}
	}

	public class CalendarAdapter : RecyclerView.Adapter {
		private Context mContext;
		private CalendarFragment mCalendar;
		private RecyclerView mListView;

		public event EventHandler<int> ItemClick;

		public CalendarAdapter(CalendarFragment calendar, RecyclerView listView)
		{
			mContext = calendar.Activity;
			mCalendar = calendar;
			mListView = listView;

			SelectedIndex = -1;
		}

		public override int ItemCount {
			get {
				return Items.Count;
			}
		}

		public int SelectedIndex {
			get;
			set;
		}

		public List<DateTime> Items {
			get;
			set;
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			return new ItemViewHolder(LayoutInflater.From(mContext).Inflate(Resource.Layout.CalendarItem, parent, false), OnClick);
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			ItemViewHolder itemHolder = holder as ItemViewHolder;
			DateTime dt = Items [position];

			itemHolder.dayNum.Text = dt.Day.ToString ();
			itemHolder.dayName.Text = mCalendar.DaysOfWeekNames [(int)dt.DayOfWeek];

			if (SelectedIndex == position) {
				itemHolder.dayNum.SetTextColor (Color.Argb (255, 255, 255, 255));
				itemHolder.dayName.SetTextColor (Color.Argb (255, 111, 175, 236));
				itemHolder.layoutBackground.SetBackgroundColor (Color.Argb (255, 25, 118, 210));
			} else {
				if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday) {
					itemHolder.dayNum.SetTextColor (Color.Argb (255, 255, 72, 72));
				} else {
					itemHolder.dayNum.SetTextColor (Color.Argb (255, 50, 101, 143));
				}
				itemHolder.dayName.SetTextColor (Color.Argb (255, 156, 181, 202));
				itemHolder.layoutBackground.SetBackgroundColor (Color.Argb (255, 231, 242, 255));
			}
		}

		public class ItemViewHolder : RecyclerView.ViewHolder 
		{
			public TextView dayNum;
			public TextView dayName;
			public RelativeLayout layoutBackground;

			public ItemViewHolder(View itemView, Action<View, int> OnClickListener) : base(itemView)
			{
				dayNum = itemView.FindViewById<TextView>(Resource.Id.dayNum);
				dayName = itemView.FindViewById<TextView>(Resource.Id.dayName);
				layoutBackground = itemView.FindViewById<RelativeLayout>(Resource.Id.layoutBackground);

				ItemView.Click += (sender, e) => OnClickListener(base.ItemView, base.Position);
			}
		}

		void OnClick(View itemView, int position)
		{
			if( ItemClick != null )
				ItemClick(itemView, position);
		}
	}

	public class CalendarTimeAdapter : BaseAdapter 
	{
		private Context mContext;
		private CalendarFragment mCalendar;
		private GridView mGridView;

		public CalendarTimeAdapter(CalendarFragment calendar, GridView gridView)
		{
			mContext = calendar.Activity;
			mGridView = gridView;
			mCalendar = calendar;

			SelectedIndex = -1;
		}

		public int SelectedIndex {
			get;
			set;
		}

		public List<string> Items {
			get;
			set;
		}

		public override int Count {
			get {
				return Items.Count;
			}
		}

		public override Java.Lang.Object GetItem (int position)
		{
			return position;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			ViewHolder holder = null;

			if (convertView == null) {
				convertView = LayoutInflater.From (mContext).Inflate (Resource.Layout.TimeItem, parent, false);

				holder = new ViewHolder ();
				holder.time = convertView.FindViewById<TextView> (Resource.Id.timeText);
				holder.layoutTime = convertView.FindViewById<RelativeLayout> (Resource.Id.layoutTime);

				convertView.Tag = holder;
			} else {
				holder = convertView.Tag as ViewHolder;
			}

			holder.time.Text = Items [position];

			if (holder.enabled && SelectedIndex == position) {
				holder.layoutTime.SetBackgroundColor (Color.Argb (255, 24, 117, 210));
				holder.time.SetTextColor(Color.Argb(255, 255, 255, 255));
			} else {
				holder.layoutTime.SetBackgroundColor (Color.Argb (255, 213, 225, 237));
				holder.time.SetTextColor(Color.Argb(255, 81, 124, 160));
			}

			if (mCalendar.Disabled != null && mCalendar.Disabled.Contains (Items [position])) {
				holder.enabled = false;
			} else {
				holder.enabled = true;
			}

			if (!holder.enabled) {
				holder.layoutTime.SetBackgroundColor (Color.Argb (125, 213, 225, 237));
				holder.time.SetTextColor(Color.Argb(125, 81, 124, 160));
			}

			return convertView;
		}

		public class ViewHolder : Java.Lang.Object {
			public TextView time;
			public RelativeLayout layoutTime;
			public bool enabled = true;
		}
	}
}


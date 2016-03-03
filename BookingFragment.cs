
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

namespace Hotels
{
	public class BookingFragment : Fragment
	{
		private TextView mTitle;
		private TextView mAnons;
		private TextView mAddr;
		private TextView mPhone;
		private TextView mReview;
		private RatingBar mRating;
		private TextView mPrice;

		private Button mChoose;
		private Button mSubmit;
		private RecyclerView mListView;
		private RecyclerView mGridView;

		private int mMonth = 0;
		private int mYear = 0;
		private DateTime mNow;

		private List<string> mTimes = new List<string>();
		private List<DateTime> mDates = new List<DateTime> ();

		private List<string> mDisabled = new List<string>();

		private int mSelectedIndex_Date = -1;
		private int mSelectedIndex_Time = -1;

		DaysAdapter mDayAdapter;
		TimeAdapter mTimeAdapter;

		private string[] mMonthNamesShort;
		private string[] mDayNamesMin;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			DateTime it_tim = new DateTime (1970, 1, 1, 9, 0, 0);

			for (int i = 0; i < 45; i++) {
				mTimes.Add (it_tim.ToString ("HH:mm"));
				it_tim = it_tim.AddMinutes (20);
			}


			mNow = DateTime.Now;
			mYear = mNow.Year;
			Month = mNow.Month;

			mDayNamesMin = Activity.Resources.GetStringArray (Resource.Array.dayNamesMin);
			mMonthNamesShort = Activity.Resources.GetStringArray (Resource.Array.monthNamesShort);
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

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.BookingFragment, container, false);


			mTitle = view.FindViewById<TextView> (Resource.Id.hotelTitle);
			mAnons = view.FindViewById<TextView> (Resource.Id.hotelAnons);
			mAddr = view.FindViewById<TextView> (Resource.Id.hotelAddr);
			mPhone = view.FindViewById<TextView> (Resource.Id.hotelPhone);
			mReview = view.FindViewById<TextView> (Resource.Id.hotelReview);
			mRating = view.FindViewById<RatingBar> (Resource.Id.hotelRating);
			mPrice = view.FindViewById<TextView> (Resource.Id.hotelPrice);

			DatabaseHelper.Connection.QueryAsync<Hotel> ("SELECT * FROM Hotel ORDER BY Id DESC LIMIT 1").ContinueWith (t => {
				Hotel record = t.Result[0];

				mTitle.Text = record.Title;
				mAnons.Text = record.Anons;
				mAddr.Text = record.Address;
				mPhone.Text = record.Phone;
				mReview.Text = Activity.Resources.GetQuantityString(Resource.Plurals.review_plural, record.Review, record.Review);
				mPrice.Text = record.Price.ToString();
				mRating.Rating = record.Rating;
			});


			mListView = view.FindViewById<RecyclerView> (Resource.Id.daysList);
			mListView.SetLayoutManager (new LinearLayoutManager (Activity, LinearLayoutManager.Horizontal, false));


			mChoose = view.FindViewById<Button> (Resource.Id.btnChoose);
			mChoose.Click += (sender, e) => {
				FragmentTransaction transaction = FragmentManager.BeginTransaction();
				var dialog = new MonthsFragment();
				dialog.OnItemSelected += OnMonthSelected;
				dialog.Show(transaction, "month select dialog");

				SelectedDateIndex = -1;
				SelectedTimeIndex = -1;
				mSubmit.Enabled = false;
			};
			mChoose.Text = MonthNamesShort [Month - 1];

			mDayAdapter = new DaysAdapter (this, mListView);
			mDayAdapter.Items = mDates;
			mDayAdapter.ItemClick += OnDayItemClick;
			mListView.SetAdapter (mDayAdapter);

			SelectCurrentDay ();

			mGridView = view.FindViewById<RecyclerView> (Resource.Id.timeList);
			mGridView.SetLayoutManager (new GridLayoutManager (Activity, 3, GridLayoutManager.Vertical, false));

			mTimeAdapter = new TimeAdapter (this, mGridView);
			mTimeAdapter.Items = mTimes;
			mGridView.SetAdapter (mTimeAdapter);
			mGridView.AddItemDecoration (new SpacingDecoration (8));

			SelectedTimeIndex = -1;

			mTimeAdapter.ItemClick += OnTimeItemClick;
			NotifyDisabled ();

			mSubmit = view.FindViewById<Button> (Resource.Id.btnSubmit);
			mSubmit.Click += OnSubmit;
			mSubmit.Enabled = false;

			return view;
		}

		void OnSubmit (object sender, EventArgs e)
		{
			if (SelectedDateIndex == -1 || SelectedTimeIndex == -1) {
				Toast.MakeText (Activity, Activity.GetString (Resource.String.empty_day_and_time), ToastLength.Long).Show ();
			} else {
				int expires = Prefs.Current.GetValueOrDefault ("Expires", 0);
				if (expires < DateTime.Now.ToUnixTimestamp ()) {
					DatabaseHelper.Connection.InsertAsync (new Booking {
						Date = mDates [SelectedDateIndex].ToString ("yyyy-MM-dd"),
						Time = mTimes [SelectedTimeIndex]
					}).ContinueWith (t => {
						mDisabled.Add(mTimes [SelectedTimeIndex]);
						Prefs.Current.AddOrUpdateValue("Expires", DateTime.Now.ToUnixTimestamp() + 86400);
						Activity.RunOnUiThread(() => {
							Toast.MakeText (Activity, string.Format(Activity.GetString (Resource.String.booking_date_and_time), mDates [SelectedDateIndex].ToString("dd.MM.yyyy"), mTimes[SelectedTimeIndex]), ToastLength.Long).Show ();
							mTimeAdapter.NotifyDataSetChanged();
						});
					});
				} else {
					Toast.MakeText (Activity, Activity.GetString (Resource.String.booking_once_per_day), ToastLength.Long).Show ();
				}
			}
		}

		void OnTimeItemClick (object sender, int index)
		{
			if (SelectedDateIndex >= 0) {
				mSubmit.Enabled = true;
				mSelectedIndex_Time = index;
			}
			mTimeAdapter.NotifyDataSetChanged ();
		}

		void OnMonthSelected (object sender, int monthIndex)
		{
			Month = monthIndex;
			mSubmit.Enabled = false;

			SelectedTimeIndex = -1;

			mChoose.Text = mMonthNamesShort [monthIndex - 1];

			SelectCurrentDay ();

			mDayAdapter.Items = mDates;
			mDayAdapter.NotifyDataSetChanged ();
		}

		void OnDayItemClick (object sender, int position)
		{
			mSubmit.Enabled = false;
			mSelectedIndex_Date = position;
			SelectedTimeIndex = -1;

			NotifyDisabled ();

			mDayAdapter.NotifyDataSetChanged ();
		}

		void SelectCurrentDay()
		{
			int index;
			SelectedDateIndex = -1;
			if ((index = mDates.FindIndex ( x => x.Date == mNow.Date )) >= 0) {
				SelectedDateIndex = index;
			}
		}

		void ReloadData()
		{
			int days = DateTime.DaysInMonth (mYear, mMonth);

			mDates.Clear ();
			var dateTime = new DateTime (mYear, mMonth, 1);
			for (int i = 0; i < days; i++) {
				mDates.Add (dateTime.AddDays (i));
			}
		}

		public async void NotifyDisabled()
		{
			mDisabled.Clear ();
			var records = await DatabaseHelper.Connection.QueryAsync<Booking> ("SELECT * FROM Booking WHERE Date = ?", mDates [SelectedDateIndex].ToString ("yyyy-MM-dd"));
			Console.WriteLine (records.Count);
			foreach (var b in records) {
				mDisabled.Add (b.Time);
			}

			mTimeAdapter.NotifyDataSetChanged ();
		}

		public bool HasDisabled(string time)
		{
			return mDisabled.Contains (time);
		}

		public int SelectedDateIndex {
			get { return mSelectedIndex_Date; }
			set {
				if (mSelectedIndex_Date != value) {
					mSelectedIndex_Date = value;
					mListView.ScrollToPosition (value);
					mDayAdapter.NotifyDataSetChanged ();
				}
			}
		}

		public int SelectedTimeIndex {
			get { return mSelectedIndex_Time; }
			set {
				if (mSelectedIndex_Time != value) {
					mSelectedIndex_Time = value;
					mTimeAdapter.NotifyDataSetChanged ();
				}
			}
		}

		public string[] MonthNamesShort {
			get {
				return mMonthNamesShort;
			}
		}

		public string[] DayNamesMin {
			get {
				return mDayNamesMin;
			}
		}
	}

	public class DaysAdapter : RecyclerView.Adapter {
		private RecyclerView mListView;
		private BookingFragment mPage;

		public event EventHandler<int> ItemClick;

		public DaysAdapter(BookingFragment page, RecyclerView listView)
		{
			mPage = page;
			mListView = listView;
		}

		public List<DateTime> Items {
			get;
			set;
		}

		public override int ItemCount {
			get {
				return Items.Count;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
		{
			var view = mPage.Activity.LayoutInflater.Inflate (Resource.Layout.DayItem, parent, false);
			return new ViewHolder(view, OnItemClick);
		}

		public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
		{
			ViewHolder mHolder = holder as ViewHolder;

			if (mPage.SelectedDateIndex == position) {
				mHolder.dayName.Selected = true;
				mHolder.dayNum.Selected = true;
				mHolder.layout.Selected = true;
			} else {
				mHolder.dayName.Selected = false;
				mHolder.dayNum.Selected = false;
				mHolder.layout.Selected = false;
			}

			mHolder.dayNum.Text = Items [position].Day.ToString ();
			mHolder.dayName.Text = mPage.DayNamesMin [(int)Items [position].DayOfWeek];
		}

		public class ViewHolder : RecyclerView.ViewHolder {
			public TextView dayNum;
			public TextView dayName;
			public RelativeLayout layout;

			public ViewHolder(View itemView, Action<View, int> ItemClickListener) : base(itemView) {
				dayNum = itemView.FindViewById<TextView>(Resource.Id.dayNum);
				dayName = itemView.FindViewById<TextView>(Resource.Id.dayName);
				layout = itemView.FindViewById<RelativeLayout>(Resource.Id.layout);

				layout.Click += (sender, e) => {
					ItemClickListener(base.ItemView, base.Position);
				};
			}
		}

		void OnItemClick(View sender, int position)
		{
			if (ItemClick != null)
				ItemClick (sender, position);
		}
	}

	public class SpacingDecoration : RecyclerView.ItemDecoration {
		private int mSpacing;

		public SpacingDecoration(int spacing) {
			this.mSpacing = spacing;
		}

		public override void GetItemOffsets (Android.Graphics.Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets (outRect, view, parent, state);

			outRect.Left = mSpacing;
			outRect.Right = mSpacing;
			outRect.Bottom = mSpacing;
			outRect.Top = mSpacing;
		}
	}

	public class TimeAdapter : RecyclerView.Adapter {
		private BookingFragment mPage;
		private RecyclerView mListView;

		public event EventHandler<int> ItemClick;

		public TimeAdapter(BookingFragment page, RecyclerView listView)
		{
			mPage = page;
			mListView = listView;
		}

		public List<string> Items {
			get;
			set;
		}

		public override int ItemCount {
			get {
				return Items.Count;
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
		{
			return new ViewHolder (mPage.Activity.LayoutInflater.Inflate (Resource.Layout.TimeItem, parent, false), OnItemClick);
		}

		public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
		{
			ViewHolder mHolder = holder as ViewHolder;

			if (mPage.SelectedTimeIndex == position) {
				mHolder.time.Selected = true;
				mHolder.layout.Selected = true;
			} else {
				mHolder.time.Selected = false;
				mHolder.layout.Selected = false;
			}

			if (mPage.HasDisabled (Items [position])) {
				mHolder.time.Enabled = false;
				mHolder.layout.Enabled = false;

				mHolder.time.Selected = false;
				mHolder.layout.Selected = false;
			} else {
				mHolder.time.Enabled = true;
				mHolder.layout.Enabled = true;
			}

			mHolder.time.Text = Items [position];
		}

		void OnItemClick(View itemView, int position)
		{
			ItemClick (itemView, position);
		}

		public class ViewHolder : RecyclerView.ViewHolder {
			public TextView time;
			public RelativeLayout layout;

			public ViewHolder(View itemView, Action<View, int> OnItemClick): base(itemView)
			{
				time = itemView.FindViewById<TextView>(Resource.Id.timeTxt);
				layout = itemView.FindViewById<RelativeLayout>(Resource.Id.layoutTime);

				layout.Click += (sender, e) => OnItemClick(base.ItemView, base.Position);
			}
		}
	}
}



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

namespace Hotels
{
	public class AboutFragment : Fragment
	{
		private TextView mTitle;
		private TextView mAnons;
		private TextView mAddr;
		private TextView mPhone;
		private RatingBar mRating;
		private TextView mReview;
		private TextView mPrice;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			var view = inflater.Inflate(Resource.Layout.AboutFragment, container, false);

			mTitle = view.FindViewById<TextView> (Resource.Id.hotelTitle);
			mAnons = view.FindViewById<TextView> (Resource.Id.hotelAnons);
			mAddr = view.FindViewById<TextView> (Resource.Id.hotelAddress);
			mPhone = view.FindViewById<TextView> (Resource.Id.hotelPhone);
			mRating = view.FindViewById<RatingBar> (Resource.Id.hotelRating);
			mReview = view.FindViewById<TextView> (Resource.Id.hotelReview);
			mPrice = view.FindViewById<TextView> (Resource.Id.hotelPrice);


			DatabaseHelper.ConnectionAsync.QueryAsync<Hotel>("SELECT * FROM Hotel ORDER BY Id DESC LIMIT 1").ContinueWith (t => {
				Hotel record = t.Result[0];

				mTitle.Text = record.Title;
				mAnons.Text = record.Announce;
				mAddr.Text = record.Address;
				mPhone.Text = record.Phone;
				mRating.Rating = record.Rating;
				mReview.Text = Activity.Resources.GetQuantityString(Resource.Plurals.plural_review, record.Review, record.Review);
				mPrice.Text = record.Price.ToString();
			});

			return view;
		}
	}
}


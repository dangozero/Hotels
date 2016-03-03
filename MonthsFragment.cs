
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
	public class MonthsFragment : DialogFragment
	{
		private ListView mListView;
		public event EventHandler<int> OnItemSelected;

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			Dialog.Window.RequestFeature (WindowFeatures.NoTitle);
			base.OnActivityCreated (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);

			List<string> mMonths = new List<string> ();
			mMonths.AddRange (Activity.Resources.GetStringArray (Resource.Array.monthNamesFull));

			// Use this to return your custom view for this Fragment
			var view = inflater.Inflate(Resource.Layout.MonthsFragment, container, false);

			mListView = view.FindViewById<ListView> (Resource.Id.monthsList);
			mListView.Adapter = new ArrayAdapter<string> (Activity, Android.Resource.Layout.SimpleListItem1, mMonths);

			mListView.ItemClick += (sender, e) => {
				OnItemSelected(sender, e.Position + 1);
				Dismiss();
			};
			return view;
		}
	}
}


using System;
using SQLite;
using System.IO;

namespace Hotels
{
	public class DatabaseHelper
	{
		private static SQLiteAsyncConnection mConnection;
		public static SQLiteAsyncConnection Connection {
			get {
				if (mConnection == null) {
					mConnection = new SQLiteAsyncConnection (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "hotels.db"));
				}
				return mConnection;
			}
		}
	}
}


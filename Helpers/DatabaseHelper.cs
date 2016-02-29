using System;
using SQLite;
using System.IO;

namespace Hotels
{
	public static class DatabaseHelper
	{
		static SQLiteAsyncConnection mConnectionAsync;
		public static SQLiteAsyncConnection ConnectionAsync {
			get {
				if (mConnectionAsync == null) {
					mConnectionAsync = new SQLiteAsyncConnection (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "hotels.db"));
				}
				return mConnectionAsync;
			}
		}

		static SQLiteConnection mConnection;
		public static SQLiteConnection Connection {
			get {
				if (mConnection == null) {
					mConnection = new SQLiteConnection (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "hotels.db"));
				}
				return mConnection;
			}
		}
	}
}


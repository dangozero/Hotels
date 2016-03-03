using System;

namespace Hotels
{
	public static class DateTimeExtension
	{
		public static int ToUnixTimestamp(this DateTime value)
		{
			return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
		}
	}
}


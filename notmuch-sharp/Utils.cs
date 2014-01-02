using System;

namespace NotMuch
{
	public static class Utils
	{
		static DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime NotmuchTimeToDateTime(long time_t)
		{
			return s_epoch.AddSeconds(time_t);
		}
	}
}

using System;

namespace NotMuchGUI
{
	static class DBNotifier
	{
		static int s_dbRefs;

		public static event Action<bool> DBOpenEvent;
		public static event Action DBCloseEvent;

		public static void AddReadRef()
		{
			if (s_dbRefs++ == 0)
				DBOpenEvent(false);
		}

		public static void AddWriteRef()
		{
			if (s_dbRefs != 0)
				throw new Exception();

			if (s_dbRefs++ == 0)
				DBOpenEvent(true);
		}

		public static void DelRef()
		{
			if (s_dbRefs == 0)
				throw new Exception();

			if (--s_dbRefs == 0)
				DBCloseEvent();
		}
	}
}

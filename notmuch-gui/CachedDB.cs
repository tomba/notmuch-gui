using System;
using NM = NotMuch;

namespace NotMuchGUI
{
	/*
	 * Get (possibly) cached NM DB. Only usable from the mainthread.
	 */
	class CachedDB : IDisposable
	{
		static NM.Database s_db;
		static int s_dbRefs;

		public NM.Database Database { get; private set; }

		public CachedDB()
		{
			MainClass.VerifyThread();
			this.Database = GetCachedDB();
		}

		public void Dispose()
		{
			MainClass.VerifyThread();
			PutCachedDB();
			GC.SuppressFinalize(this);
		}

		~CachedDB()
		{
			PutCachedDB();
		}

		static NM.Database GetCachedDB()
		{
			MainClass.VerifyThread();

			if (s_dbRefs == 0)
				DBNotifier.AddReadRef();

			if (s_db == null)
			{
				Console.WriteLine("open DB");
				s_db = MainClass.OpenDB();

				GLib.Timeout.Add(1000, () =>
					{
						if (s_dbRefs > 0)
							return true;

						Console.WriteLine("close DB");

						s_db.Dispose();
						s_db = null;
						return false;
					});
			}

			s_dbRefs++;
			return s_db;
		}

		static void PutCachedDB()
		{
			s_dbRefs--;
			if (s_dbRefs < 0)
				throw new Exception();

			if (s_dbRefs == 0)
				DBNotifier.DelRef();
		}
	}
}


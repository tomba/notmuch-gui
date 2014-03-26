using System;
using System.Collections.Generic;
using NM = NotMuch;

namespace NotMuchGUI
{
	class CachedDB : IDisposable
	{
		static object s_lock = new object();
		static NM.Database s_unusedDB;
		static DateTime s_unusedDateStamp;
		static int s_readers;
		static int s_writers;

		public static event Action<bool> DBOpenEvent;
		public static event Action DBCloseEvent;

		public NM.Database Database { get; private set; }

		bool m_writable;

		public CachedDB()
				: this(false)
		{
		}

		public CachedDB(bool writable)
		{
			m_writable = writable;
			bool notify = false;

			lock (s_lock)
			{
				if (s_writers > 0)
					throw new Exception();

				if (writable && s_readers > 0)
					throw new Exception();

				if (s_unusedDB != null)
				{
					if (writable)
					{
						Console.WriteLine("flush cached DB");
						s_unusedDB.Dispose();
						s_unusedDB = null;
					}
					else
					{
						Console.WriteLine("returning cached DB");
						this.Database = s_unusedDB;
						s_unusedDB = null;
					}
				}

				if (this.Database == null)
				{
					NM.Status status;

					var mode = writable ? NM.DatabaseMode.READ_WRITE : NM.DatabaseMode.READ_ONLY;

					Console.WriteLine("returning new DB {0}", mode);

					var db = NM.Database.Open(MainClass.DatabasePath, mode, out status);
					if (db == null)
						throw new Exception();

					this.Database = db;
				}

				if (writable)
				{
					if (s_writers++ == 0)
						notify = true;
				}
				else
				{
					if (s_readers++ == 0)
						notify = true;
				}
			}

			if (notify)
				DBOpenEvent(writable);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			bool notify = false;

			lock (s_lock)
			{
				if (m_writable == false && s_unusedDB == null)
				{
					Console.WriteLine("caching DB");
					s_unusedDB = this.Database;
					s_unusedDateStamp = DateTime.UtcNow;
					this.Database = null;

					GLib.Timeout.Add(1000, DisposeCheckTimeout);
				}
				else
				{
					Console.WriteLine("discarding DB");
					this.Database.Dispose();
					this.Database = null;
				}

				if (m_writable)
				{
					if (--s_writers == 0)
						notify = true;
				}
				else
				{
					if (--s_readers == 0)
						notify = true;
				}
			}

			if (notify)
				DBCloseEvent();
		}

		~CachedDB()
		{
			this.Database.Dispose();

			throw new Exception();
		}

		static bool DisposeCheckTimeout()
		{
			lock (s_lock)
			{
				if (s_unusedDB == null)
					return false;

				var diff = DateTime.UtcNow - s_unusedDateStamp;

				if (diff.TotalMilliseconds >= 1000)
				{
					Console.WriteLine("disposing by timeout");
					s_unusedDB.Dispose();
					s_unusedDB = null;
				}
			}

			return false;
		}
	}
}

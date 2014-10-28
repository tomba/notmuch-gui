using System;
using System.Collections.Generic;
using NM = NotMuch;
using System.Threading;

namespace NotMuchGUI
{
	class CachedDB : IDisposable
	{
		static object s_lock = new object();
		static NM.Database s_unusedDB;
		static DateTime s_unusedDateStamp;
		static int s_readers;
		static int s_writers;
		static ReaderWriterLockSlim s_rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

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

			if (writable)
				s_rwLock.EnterWriteLock();
			else
				s_rwLock.EnterReadLock();

			lock (s_lock)
			{
				if (s_unusedDB != null)
				{
					if (writable)
					{
						s_unusedDB.Dispose();
						s_unusedDB = null;
					}
					else
					{
						this.Database = s_unusedDB;
						s_unusedDB = null;
					}
				}

				if (s_writers == 0 && s_readers == 0 && DBOpenEvent != null)
					DBOpenEvent(writable);

				if (writable)
					s_writers++;
				else
					s_readers++;
			}

			if (this.Database == null)
			{
				NM.Status status;

				var mode = writable ? NM.DatabaseMode.READ_WRITE : NM.DatabaseMode.READ_ONLY;

				var db = NM.Database.Open(MainClass.DatabasePath, mode, out status);
				if (db == null)
					throw new Exception();

				this.Database = db;
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);

			lock (s_lock)
			{
				if (m_writable == false && s_unusedDB == null)
				{
					s_unusedDB = this.Database;
					s_unusedDateStamp = DateTime.UtcNow;
					this.Database = null;

					GLib.Timeout.Add(1000, DisposeCheckTimeout);
				}

				if (m_writable)
					s_writers--;
				else
					s_readers--;

				if (s_writers == 0 && s_readers == 0 && DBCloseEvent != null)
					DBCloseEvent();
			}

			if (this.Database != null)
			{
				this.Database.Dispose();
				this.Database = null;
			}

			if (m_writable)
				s_rwLock.ExitWriteLock();
			else
				s_rwLock.ExitReadLock();
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
					s_unusedDB.Dispose();
					s_unusedDB = null;
				}
			}

			return false;
		}
	}
}

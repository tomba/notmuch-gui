using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public class Query : IDisposable
	{
		Database m_db;
		IntPtr m_handle;

		internal Query(Database db, IntPtr handle)
		{
			m_db = db;
			m_handle = handle;
		}

		~Query()
		{
			Debug.WriteLine("~Query");

			Dispose(false);
		}

		public void Dispose()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			m_db.OnQueryDisposed(this);
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (m_handle != IntPtr.Zero)
			{
				notmuch_query_destroy(m_handle);
				m_handle = IntPtr.Zero;
			}
		}

		public int CountMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			return (int)notmuch_query_count_messages(m_handle);
		}

		public int CountThreads()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			return (int)notmuch_query_count_threads(m_handle);
		}

		public Messages SearchMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr msgsP = notmuch_query_search_messages(m_handle);

			return new Messages(msgsP);
		}

		public Threads SearchThreads()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr msgsP = notmuch_query_search_threads(m_handle);

			return new Threads(msgsP);
		}

		public SortOrder Sort
		{
			get { return (SortOrder)notmuch_query_get_sort(m_handle); }
			set { notmuch_query_set_sort(m_handle, (int)value); }
		}

		[DllImport("libnotmuch")]
		static extern void notmuch_query_destroy(IntPtr query);

		[DllImport("libnotmuch")]
		static extern uint notmuch_query_count_messages(IntPtr query);

		[DllImport("libnotmuch")]
		static extern uint notmuch_query_count_threads(IntPtr query);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_query_search_messages(IntPtr query);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_query_search_threads(IntPtr query);

		[DllImport("libnotmuch")]
		static extern void notmuch_query_set_sort(IntPtr query, int sort);

		[DllImport("libnotmuch")]
		static extern int notmuch_query_get_sort(IntPtr query);
	}
}

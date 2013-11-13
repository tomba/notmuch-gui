using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Query : IDisposable
	{
		IntPtr m_handle;

		internal Query(IntPtr handle)
		{
			m_handle = handle;
		}

		~Query ()
		{
			Dispose(false);
		}

		public void Dispose()
		{
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

		public int Count
		{
			get
			{
				return (int)notmuch_query_count_messages(m_handle);
			}
		}

		public Messages SearchMessages()
		{
			IntPtr msgsP = notmuch_query_search_messages(m_handle);

			return new Messages(msgsP);
		}

		public Threads SearchThreads()
		{
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
		static extern IntPtr notmuch_query_search_messages(IntPtr query);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_query_search_threads(IntPtr query);

		[DllImport("libnotmuch")]
		static extern void notmuch_query_set_sort(IntPtr query, int sort);

		[DllImport("libnotmuch")]
		static extern int notmuch_query_get_sort(IntPtr query);
	}
}

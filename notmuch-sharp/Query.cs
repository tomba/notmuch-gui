using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace NotMuch
{
	public sealed class Query : IDisposable
	{
		IntPtr m_handle;

		internal Query(IntPtr handle)
		{
			m_handle = handle;
		}

		public void Dispose()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			notmuch_query_destroy(m_handle);
			m_handle = IntPtr.Zero;
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

		public IEnumerable<Message> SearchMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hMessages = notmuch_query_search_messages(m_handle);

			while (Messages.Valid(hMessages))
			{
				yield return Messages.Get(hMessages);
				Messages.MoveToNext(hMessages);
			}
		}

		public IEnumerable<Thread> SearchThreads()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hThreads = notmuch_query_search_threads(m_handle);

			while (Threads.Valid(hThreads))
			{
				yield return Threads.Get(hThreads);
				Threads.MoveToNext(hThreads);
			}
		}

		public SortOrder Sort
		{
			get { return (SortOrder)notmuch_query_get_sort(m_handle); }
			set { notmuch_query_set_sort(m_handle, (int)value); }
		}

		public Exclude Exclude
		{
			set { notmuch_query_set_omit_excluded(m_handle, value); }
		}

		public void AddTagExclude(string tag)
		{
			if (tag == null)
				throw new ArgumentNullException("tag");

			notmuch_query_add_tag_exclude(m_handle, tag);
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

		[DllImport("libnotmuch")]
		static extern void notmuch_query_set_omit_excluded(IntPtr query, Exclude omit_excluded);

		[DllImport("libnotmuch")]
		static extern void notmuch_query_add_tag_exclude(IntPtr query, string tag);
	}

	public enum Exclude
	{
		FLAG,
		TRUE,
		FALSE,
		ALL
	}
}

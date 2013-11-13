using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Query : DisposableBase
	{
		internal Query(IntPtr handle)
			: base(handle)
		{
		}

		public int Count
		{
			get
			{
				return (int)notmuch_query_count_messages(this.Handle);
			}
		}

		public Messages SearchMessages()
		{
			IntPtr msgsP = notmuch_query_search_messages(this.Handle);

			return new Messages(msgsP);
		}

		public Threads SearchThreads()
		{
			IntPtr msgsP = notmuch_query_search_threads(this.Handle);

			return new Threads(msgsP);
		}

		protected override void DestroyHandle()
		{
			notmuch_query_destroy(this.Handle);
		}

		public SortOrder Sort
		{
			get { return (SortOrder)notmuch_query_get_sort(this.Handle); }
			set { notmuch_query_set_sort(this.Handle, (int)value); }
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

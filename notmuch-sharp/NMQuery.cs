using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class NMQuery : DisposableBase
	{
		public static NMQuery Create(NMDatabase db, string queryString)
		{
			IntPtr query = Native.notmuch_query_create(db.Handle, queryString);

			return new NMQuery(query);
		}

		NMQuery(IntPtr handle)
			: base(handle)
		{
		}

		public int Count
		{
			get
			{
				return (int)Native.notmuch_query_count_messages(this.Handle);
			}
		}

		public NMMessages SearchMessages()
		{
			IntPtr msgsP = Native.notmuch_query_search_messages(this.Handle);

			return new NMMessages(msgsP);
		}

		public NMThreads SearchThreads()
		{
			IntPtr msgsP = Native.notmuch_query_search_threads(this.Handle);

			return new NMThreads(msgsP);
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_query_destroy(this.Handle);
		}
	}
}

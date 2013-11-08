using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Query : DisposableBase
	{
		public static Query Create(Database db, string queryString)
		{
			IntPtr query = Native.notmuch_query_create(db.Handle, queryString);

			return new Query(query);
		}

		Query(IntPtr handle)
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

		public Messages SearchMessages()
		{
			IntPtr msgsP = Native.notmuch_query_search_messages(this.Handle);

			return new Messages(msgsP);
		}

		public Threads SearchThreads()
		{
			IntPtr msgsP = Native.notmuch_query_search_threads(this.Handle);

			return new Threads(msgsP);
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_query_destroy(this.Handle);
		}
	}
}

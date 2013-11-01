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

		public Messages Search()
		{
			IntPtr msgsP = Native.notmuch_query_search_messages(this.Handle);

			return new Messages(msgsP);
		}

		public void Run()
		{
			var msgs = Search();

			foreach (var msg in msgs)
			{
				var fn = msg.FileName;
				var from = msg.GetHeader("From");

				Console.WriteLine("{0}, {1}", fn, from);
			}
			/*
			while (msgs.Valid)
			{
				var msg = msgs.Current;

				var fn = msg.FileName;

				Console.WriteLine(fn);

				msg.Dispose();

				msgs.Next();
			}
*/
			//msgs.Dispose();
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_query_destroy(this.Handle);
		}
	}
}

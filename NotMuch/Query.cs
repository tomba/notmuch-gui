using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
    public class Query
    {
        public Query(Database db, string queryString)
        {
            IntPtr query = Native.notmuch_query_create(db.Handle, queryString);

            IntPtr msgs = Native.notmuch_query_search_messages(query);

            while (Native.notmuch_messages_valid(msgs))
            {
                Console.WriteLine("loop");

                var msgP = Native.notmuch_messages_get(msgs);

                var msg = new Message(msgP);

                var fn = msg.FileName;

                Console.WriteLine(fn);

                msg.Dispose();

                Native.notmuch_messages_move_to_next(msgs);
            }

            Native.notmuch_query_destroy(query);
        }
    }
}


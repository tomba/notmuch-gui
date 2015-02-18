using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	static class Messages
	{
		public static bool Valid(IntPtr messages)
		{
			return notmuch_messages_valid(messages);
		}

		public static void MoveToNext(IntPtr messages)
		{
			notmuch_messages_move_to_next(messages);
		}

		public static Message Get(IntPtr messages)
		{
			return new Message(notmuch_messages_get(messages));
		}

		[DllImport("libnotmuch")]
		static extern bool notmuch_messages_destroy(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern bool notmuch_messages_valid(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern void notmuch_messages_move_to_next(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_messages_get(IntPtr messages);
	}
}


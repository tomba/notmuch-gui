using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	class Threads
	{
		public static bool Valid(IntPtr threads)
		{
			return notmuch_threads_valid(threads);
		}

		public static void MoveToNext(IntPtr threads)
		{
			notmuch_threads_move_to_next(threads);
		}

		public static Thread Get(IntPtr threads)
		{
			return new Thread(notmuch_threads_get(threads));
		}

		[DllImport("libnotmuch")]
		static extern void notmuch_threads_destroy(IntPtr threads);

		[DllImport("libnotmuch")]
		static extern bool notmuch_threads_valid(IntPtr threads);

		[DllImport("libnotmuch")]
		static extern void notmuch_threads_move_to_next(IntPtr threads);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_threads_get(IntPtr threads);
	}
}

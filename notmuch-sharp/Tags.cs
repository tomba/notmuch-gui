using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	static class Tags
	{
		public static bool Valid(IntPtr tags)
		{
			return notmuch_tags_valid(tags);
		}

		public static void MoveToNext(IntPtr tags)
		{
			notmuch_tags_move_to_next(tags);
		}

		public static string Get(IntPtr tags)
		{
			return Marshal.PtrToStringAnsi(notmuch_tags_get(tags));
		}

		[DllImport("libnotmuch")]
		static extern void notmuch_tags_destroy(IntPtr tags);

		[DllImport("libnotmuch")]
		static extern bool notmuch_tags_valid(IntPtr tags);

		[DllImport("libnotmuch")]
		static extern void notmuch_tags_move_to_next(IntPtr tags);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_tags_get(IntPtr tags);
	}
}

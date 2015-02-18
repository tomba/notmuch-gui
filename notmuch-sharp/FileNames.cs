using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	static class FileNames
	{
		public static bool Valid(IntPtr filenames)
		{
			return notmuch_filenames_valid(filenames);
		}

		public static void MoveToNext(IntPtr filenames)
		{
			notmuch_filenames_move_to_next(filenames);
		}

		public static string Get(IntPtr filenames)
		{
			return Marshal.PtrToStringAnsi(notmuch_filenames_get(filenames));
		}

		[DllImport("libnotmuch")]
		static extern void notmuch_filenames_destroy(IntPtr filenames);

		[DllImport("libnotmuch")]
		static extern bool notmuch_filenames_valid(IntPtr filenames);

		[DllImport("libnotmuch")]
		static extern void notmuch_filenames_move_to_next(IntPtr filenames);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_filenames_get(IntPtr filenames);
	}
}

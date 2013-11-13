using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Tags
	{
		IntPtr Handle;

		internal Tags(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return notmuch_tags_valid(this.Handle); } }

		public string Current
		{ 
			get
			{
				IntPtr ptr = notmuch_tags_get(this.Handle);
				return Marshal.PtrToStringAnsi(ptr);
			}
		}

		public void Next()
		{
			notmuch_tags_move_to_next(this.Handle);
		}

		[DllImport("libnotmuch")]
		static extern bool notmuch_tags_valid(IntPtr tags);

		[DllImport("libnotmuch")]
		static extern void notmuch_tags_move_to_next(IntPtr tags);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_tags_get(IntPtr tags);
	}
}

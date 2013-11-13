using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Tags
	{
		IntPtr m_handle;

		internal Tags(IntPtr handle)
		{
			m_handle = handle;
		}

		public bool Valid { get { return notmuch_tags_valid(m_handle); } }

		public string Current
		{ 
			get
			{
				IntPtr ptr = notmuch_tags_get(m_handle);
				return Marshal.PtrToStringAnsi(ptr);
			}
		}

		public void Next()
		{
			notmuch_tags_move_to_next(m_handle);
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

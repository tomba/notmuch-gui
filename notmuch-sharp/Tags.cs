using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public struct Tags : IEnumerable<string>
	{
		IntPtr m_handle;

		internal Tags(IntPtr handle)
		{
			m_handle = handle;
		}

		#region IEnumerable implementation

		public IEnumerator<string> GetEnumerator()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			while (notmuch_tags_valid(m_handle))
			{
				IntPtr ptr = notmuch_tags_get(m_handle);
				yield return Marshal.PtrToStringAnsi(ptr);
				notmuch_tags_move_to_next(m_handle);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

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

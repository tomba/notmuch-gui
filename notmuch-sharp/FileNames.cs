using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public struct FileNames : IEnumerable<string>
	{
		IntPtr m_handle;

		internal FileNames(IntPtr handle)
		{
			m_handle = handle;
		}

		#region IEnumerable implementation

		public IEnumerator<string> GetEnumerator()
		{
			if (m_handle == IntPtr.Zero)
				yield break;

			while (notmuch_filenames_valid(m_handle))
			{
				IntPtr ptr = notmuch_filenames_get(m_handle);
				yield return Marshal.PtrToStringAnsi(ptr);
				notmuch_filenames_move_to_next(m_handle);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

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

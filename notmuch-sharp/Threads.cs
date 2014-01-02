using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public struct Threads : IEnumerable<Thread>
	{
		IntPtr m_handle;

		internal Threads(IntPtr handle)
		{
			m_handle = handle;
		}

		#region IEnumerable implementation

		public IEnumerator<Thread> GetEnumerator()
		{
			if (m_handle == IntPtr.Zero)
				yield break;

			while (notmuch_threads_valid(m_handle))
			{
				yield return new Thread(notmuch_threads_get(m_handle));
				notmuch_threads_move_to_next(m_handle);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

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

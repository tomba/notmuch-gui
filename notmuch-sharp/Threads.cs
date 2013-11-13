using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Threads
	{
		IntPtr m_handle;

		internal Threads(IntPtr handle)
		{
			m_handle = handle;
		}

		public bool Valid { get { return notmuch_threads_valid(m_handle); } }

		public Thread Current
		{ 
			get
			{
				return new Thread(notmuch_threads_get(m_handle));
			}
		}

		public void Next()
		{
			notmuch_threads_move_to_next(m_handle);
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

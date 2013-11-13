using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Threads
	{
		IntPtr Handle;

		internal Threads(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return notmuch_threads_valid(this.Handle); } }

		public Thread Current
		{ 
			get
			{
				return new Thread(notmuch_threads_get(this.Handle));
			}
		}

		public void Next()
		{
			notmuch_threads_move_to_next(this.Handle);
		}

		[DllImport("libnotmuch")]
		static extern bool notmuch_threads_valid(IntPtr threads);

		[DllImport("libnotmuch")]
		static extern void notmuch_threads_move_to_next(IntPtr threads);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_threads_get(IntPtr threads);
	}
}

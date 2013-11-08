using System;
using System.Collections.Generic;

namespace NotMuch
{
	public struct Threads
	{
		internal IntPtr Handle;

		public Threads(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return Native.notmuch_threads_valid(this.Handle); } }

		public Thread Current
		{ 
			get
			{
				return new Thread(Native.notmuch_threads_get(this.Handle));
			}
		}

		public void Next()
		{
			Native.notmuch_threads_move_to_next(this.Handle);
		}
	}
}

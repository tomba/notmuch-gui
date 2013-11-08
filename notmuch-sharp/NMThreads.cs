using System;
using System.Collections.Generic;

namespace NotMuch
{
	public struct NMThreads
	{
		internal IntPtr Handle;

		public NMThreads(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return Native.notmuch_threads_valid(this.Handle); } }

		public NMThread Current
		{ 
			get
			{
				return new NMThread(Native.notmuch_threads_get(this.Handle));
			}
		}

		public void Next()
		{
			Native.notmuch_threads_move_to_next(this.Handle);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Tags
	{
		internal IntPtr Handle;

		public Tags(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return Native.notmuch_tags_valid(this.Handle); } }

		public string Current
		{ 
			get
			{
				IntPtr ptr = Native.notmuch_tags_get(this.Handle);
				return Marshal.PtrToStringAnsi(ptr);
			}
		}

		public void Next()
		{
			Native.notmuch_tags_move_to_next(this.Handle);
		}
	}
}

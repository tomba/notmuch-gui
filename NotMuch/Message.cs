using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Message : DisposableBase
	{
		internal Message(IntPtr ptr)
            : base(ptr)
		{
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_message_destroy(this.Handle);
		}

		public string FileName
		{
			get
			{
				IntPtr sp = Native.notmuch_message_get_filename(this.Handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public string GetHeader(string name)
		{
			IntPtr sp = Native.notmuch_message_get_header(this.Handle, name);

			return Marshal.PtrToStringAnsi(sp);

		}
	}
}


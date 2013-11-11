using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Thread
	{
		internal IntPtr Handle;

		internal Thread(IntPtr handle)
		{
			Handle = handle;
		}

		public string Id
		{
			get
			{
				IntPtr p = Native.notmuch_thread_get_thread_id(this.Handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public int TotalMessages
		{
			get
			{
				return Native.notmuch_thread_get_total_messages(this.Handle);
			}
		}

		public Messages GetToplevelMessages()
		{
			IntPtr msgsP = Native.notmuch_thread_get_toplevel_messages(this.Handle);

			return new Messages(msgsP);
		}
	}
}


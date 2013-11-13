using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Thread
	{
		IntPtr Handle;

		internal Thread(IntPtr handle)
		{
			Handle = handle;
		}

		public string Id
		{
			get
			{
				IntPtr p = notmuch_thread_get_thread_id(this.Handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public int TotalMessages
		{
			get
			{
				return notmuch_thread_get_total_messages(this.Handle);
			}
		}

		public Messages GetToplevelMessages()
		{
			IntPtr msgsP = notmuch_thread_get_toplevel_messages(this.Handle);

			return new Messages(msgsP);
		}

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_thread_id(IntPtr query);

		[DllImport("libnotmuch")]
		static extern int notmuch_thread_get_total_messages(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_toplevel_messages(IntPtr query);
	}
}


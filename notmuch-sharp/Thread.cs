using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Thread
	{
		IntPtr m_handle;

		internal Thread(IntPtr handle)
		{
			m_handle = handle;
		}

		public string Id
		{
			get
			{
				IntPtr p = notmuch_thread_get_thread_id(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public int TotalMessages
		{
			get
			{
				return notmuch_thread_get_total_messages(m_handle);
			}
		}

		public Messages GetToplevelMessages()
		{
			IntPtr msgsP = notmuch_thread_get_toplevel_messages(m_handle);

			return new Messages(msgsP);
		}

		[DllImport("libnotmuch")]
		static extern void notmuch_thread_destroy(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_thread_id(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern int notmuch_thread_get_total_messages(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_toplevel_messages(IntPtr thread);
	}
}


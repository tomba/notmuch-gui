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

		public string ID
		{
			get
			{
				IntPtr p = notmuch_thread_get_thread_id(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public int GetTotalMessages()
		{
			return notmuch_thread_get_total_messages(m_handle);
		}

		/**
		 * Get a notmuch_messages_t iterator for the top-level messages in
		 * 'thread' in oldest-first order.
		 *
		 * This iterator will not necessarily iterate over all of the messages
		 * in the thread. It will only iterate over the messages in the thread
		 * which are not replies to other messages in the thread.
		 *
		 * The returned list will be destroyed when the thread is destroyed.
		 */
		public Messages GetToplevelMessages()
		{
			IntPtr msgsP = notmuch_thread_get_toplevel_messages(m_handle);

			return new Messages(msgsP);
		}

		/**
		 * Get a notmuch_messages_t iterator for all messages in 'thread' in
		 * oldest-first order.
		 *
		 * The returned list will be destroyed when the thread is destroyed.
		 */
		public Messages GetMessages()
		{
			IntPtr msgsP = notmuch_thread_get_messages(m_handle);

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

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_messages(IntPtr thread);
	}
}


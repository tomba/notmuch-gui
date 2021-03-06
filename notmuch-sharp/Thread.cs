using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

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
				Debug.Assert(m_handle != IntPtr.Zero);
				IntPtr p = notmuch_thread_get_thread_id(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public int GetTotalMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);
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
		public IEnumerable<Message> GetToplevelMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hMessages = notmuch_thread_get_toplevel_messages(m_handle);

			while (Messages.Valid(hMessages))
			{
				yield return Messages.Get(hMessages);
				Messages.MoveToNext(hMessages);
			}
		}

		/**
		 * Get a notmuch_messages_t iterator for all messages in 'thread' in
		 * oldest-first order.
		 *
		 * The returned list will be destroyed when the thread is destroyed.
		 */
		public IEnumerable<Message> GetMessages()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hMessages = notmuch_thread_get_messages(m_handle);

			while (Messages.Valid(hMessages))
			{
				yield return Messages.Get(hMessages);
				Messages.MoveToNext(hMessages);
			}
		}

		public string Authors
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);
				IntPtr p = notmuch_thread_get_authors(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public string Subject
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);
				IntPtr p = notmuch_thread_get_subject(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		/// <summary>
		/// Returns the date in UTC
		/// </summary>
		public DateTime NewestDate
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr time_t = notmuch_thread_get_newest_date(m_handle);

				return Utils.NotmuchTimeToDateTime((long)time_t);
			}
		}

		public IEnumerable<string> GetTags()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hTags = notmuch_thread_get_tags(m_handle);

			while (Tags.Valid(hTags))
			{
				var str = Tags.Get(hTags);
				str = string.Intern(str);
				yield return str;
				Tags.MoveToNext(hTags);
			}
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

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_authors(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_subject(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_newest_date(IntPtr thread);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_thread_get_tags(IntPtr thread);
	}
}


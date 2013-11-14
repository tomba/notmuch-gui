using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public struct Message
	{
		IntPtr m_handle;
		public static Message NullMessage = new Message(IntPtr.Zero);

		internal Message(IntPtr handle)
		{
			m_handle = handle;
		}

		public void DestroyHandle()
		{
			Debug.Assert(m_handle != IntPtr.Zero);
			notmuch_message_destroy(m_handle);
		}

		public bool IsNull { get { return m_handle == IntPtr.Zero; } }

		public string FileName
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_filename(m_handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public Tags GetTags()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr tags = notmuch_message_get_tags(m_handle);

			return new Tags(tags);
		}

		public Status AddTag(string tag)
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			return notmuch_message_add_tag(m_handle, tag);
		}

		public Status RemoveTag(string tag)
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			return notmuch_message_remove_tag(m_handle, tag);
		}

		public string GetHeader(string name)
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr sp = notmuch_message_get_header(m_handle, name);

			return Marshal.PtrToStringAnsi(sp);
		}

		static DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Returns the date in UTC
		/// </summary>
		public DateTime Date
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr time_t = notmuch_message_get_date(m_handle);

				return s_epoch.AddSeconds((long)time_t);
			}
		}

		public long DateStamp
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr time_t = notmuch_message_get_date(m_handle);

				return (long)time_t;
			}
		}

		public string Id
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_message_id(m_handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public Messages GetReplies()
		{
			IntPtr msgsP = notmuch_message_get_replies(m_handle);

			return new Messages(msgsP);
		}

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_destroy(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_message_id(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_date(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_filename(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_header(IntPtr message, string header);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_tags(IntPtr message);

		[DllImport("libnotmuch")]
		static extern Status notmuch_message_add_tag(IntPtr message, string tag);

		[DllImport("libnotmuch")]
		static extern Status notmuch_message_remove_tag(IntPtr message, string tag);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_replies(IntPtr message);
	}
}

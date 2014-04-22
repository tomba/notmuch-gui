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

		public FileNames GetFileNames()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr filenames = notmuch_message_get_filenames(m_handle);

			return new FileNames(filenames);
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

		public string From { get { return GetHeader("from"); } }

		public string To { get { return GetHeader("to"); } }

		public string Cc { get { return GetHeader("cc"); } }

		public string Subject { get { return GetHeader("subject"); } }

		/// <summary>
		/// Returns the date in UTC
		/// </summary>
		public DateTime Date
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr time_t = notmuch_message_get_date(m_handle);

				return Utils.NotmuchTimeToDateTime((long)time_t);
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

		public string ID
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_message_id(m_handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public string ThreadID
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_thread_id(m_handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public Messages GetReplies()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr msgsP = notmuch_message_get_replies(m_handle);

			return new Messages(msgsP);
		}

		public bool GetFlag(MessageFlag flag)
		{
			return notmuch_message_get_flag(m_handle, flag);
		}

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_destroy(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_message_id(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_thread_id(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_date(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_filename(IntPtr message);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_message_get_filenames(IntPtr message);

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

		[DllImport("libnotmuch")]
		static extern bool notmuch_message_get_flag(IntPtr message, MessageFlag flag);
	}

	public enum MessageFlag
	{
		MATCH,
		EXCLUDED
	}
}

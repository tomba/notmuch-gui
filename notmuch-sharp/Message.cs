using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

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

		public IEnumerable<string> GetFileNames()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hFileNames = notmuch_message_get_filenames(m_handle);

			while (FileNames.Valid(hFileNames))
			{
				yield return FileNames.Get(hFileNames);
				FileNames.MoveToNext(hFileNames);
			}
		}

		public IEnumerable<string> GetTags()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hTags = notmuch_message_get_tags(m_handle);

			while (Tags.Valid(hTags))
			{
				var str = Tags.Get(hTags);
				str = string.Intern(str);
				yield return str;
				Tags.MoveToNext(hTags);
			}
		}

		public Status AddTag(string tag)
		{
			if (tag == null)
				throw new ArgumentNullException("tag");

			Debug.Assert(m_handle != IntPtr.Zero);

			return notmuch_message_add_tag(m_handle, tag);
		}

		public Status RemoveTag(string tag)
		{
			if (tag == null)
				throw new ArgumentNullException("tag");

			Debug.Assert(m_handle != IntPtr.Zero);

			return notmuch_message_remove_tag(m_handle, tag);
		}

		public string GetHeader(string name)
		{
			if (name == null)
				throw new ArgumentNullException("name");

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

		public IEnumerable<Message> GetReplies()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hMessages = notmuch_message_get_replies(m_handle);

			while (Messages.Valid(hMessages))
			{
				yield return Messages.Get(hMessages);
				Messages.MoveToNext(hMessages);
			}
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

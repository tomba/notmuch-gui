using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace NotMuch
{
	public struct Message
	{
		internal IntPtr Handle;
		public static Message NullMessage = new Message(IntPtr.Zero);

		internal Message(IntPtr handle)
		{
			Handle = handle;
		}

		public void DestroyHandle()
		{
			Debug.Assert(this.Handle != IntPtr.Zero);
			notmuch_message_destroy(this.Handle);
		}

		public bool IsNull { get { return this.Handle == IntPtr.Zero; } }

		public string FileName
		{
			get
			{
				Debug.Assert(this.Handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_filename(this.Handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public Tags GetTags()
		{
			Debug.Assert(this.Handle != IntPtr.Zero);

			IntPtr tags = notmuch_message_get_tags(this.Handle);

			return new Tags(tags);
		}

		public string GetHeader(string name)
		{
			Debug.Assert(this.Handle != IntPtr.Zero);

			IntPtr sp = notmuch_message_get_header(this.Handle, name);

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
				Debug.Assert(this.Handle != IntPtr.Zero);

				IntPtr time_t = notmuch_message_get_date(this.Handle);

				return s_epoch.AddSeconds((long)time_t);
			}
		}

		public long DateStamp
		{
			get
			{
				Debug.Assert(this.Handle != IntPtr.Zero);

				IntPtr time_t = notmuch_message_get_date(this.Handle);

				return (long)time_t;
			}
		}

		public string Id
		{
			get
			{
				Debug.Assert(this.Handle != IntPtr.Zero);

				IntPtr sp = notmuch_message_get_message_id(this.Handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}

		public Messages GetReplies()
		{
			IntPtr msgsP = notmuch_message_get_replies(this.Handle);

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
		static extern IntPtr notmuch_message_get_replies(IntPtr threads);
	}
}

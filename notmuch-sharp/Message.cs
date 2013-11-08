using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Message
	{
		internal IntPtr Handle;

		internal Message(IntPtr handle)
		{
			Handle = handle;
		}

		public void DestroyHandle()
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

		public Tags GetTags()
		{
			IntPtr tags = Native.notmuch_message_get_tags(this.Handle);

			return new Tags(tags);
		}

		public string GetHeader(string name)
		{
			IntPtr sp = Native.notmuch_message_get_header(this.Handle, name);

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
				IntPtr time_t = Native.notmuch_message_get_date(this.Handle);

				return s_epoch.AddSeconds((long)time_t);
			}
		}

		public long DateStamp
		{
			get
			{
				IntPtr time_t = Native.notmuch_message_get_date(this.Handle);

				return (long)time_t;
			}
		}

		public string Id
		{
			get
			{
				IntPtr sp = Native.notmuch_message_get_message_id(this.Handle);

				return Marshal.PtrToStringAnsi(sp);
			}
		}
	}
}

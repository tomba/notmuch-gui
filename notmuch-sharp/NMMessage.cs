using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct NMMessage
	{
		internal IntPtr Handle;

		internal NMMessage(IntPtr handle)
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

		public string GetHeader(string name)
		{
			IntPtr sp = Native.notmuch_message_get_header(this.Handle, name);

			return Marshal.PtrToStringAnsi(sp);
		}

		public DateTime Date
		{
			get
			{
				var dstr = GetHeader("Date");
				return DateTime.Parse(dstr);
			}
		}

		public IntPtr Date2
		{
			get
			{
				long time_t = Native.notmuch_message_get_date(this.Handle);
				return (IntPtr)time_t;
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

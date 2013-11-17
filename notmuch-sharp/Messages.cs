using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Messages : IEnumerable<Message>
	{
		IntPtr m_handle;

		internal Messages(IntPtr handle)
		{
			m_handle = handle;
		}

		#region IEnumerable implementation

		public IEnumerator<Message> GetEnumerator()
		{
			while (notmuch_messages_valid(m_handle))
			{
				yield return new Message(notmuch_messages_get(m_handle));
				notmuch_messages_move_to_next(m_handle);
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		[DllImport("libnotmuch")]
		static extern bool notmuch_messages_destroy(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern bool notmuch_messages_valid(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern void notmuch_messages_move_to_next(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_messages_get(IntPtr messages);
	}
}


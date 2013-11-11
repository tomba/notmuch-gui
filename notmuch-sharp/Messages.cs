using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Messages : IEnumerable<Message>
	{
		internal IntPtr Handle;

		public Messages(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return notmuch_messages_valid(this.Handle); } }

		public Message Current
		{ 
			get
			{
				return new Message(notmuch_messages_get(this.Handle));
			}
		}

		public void Next()
		{
			notmuch_messages_move_to_next(this.Handle);
		}

		#region IEnumerable implementation

		public IEnumerator<Message> GetEnumerator()
		{
			return new MyEnumerator(this);
		}

		#endregion

		#region IEnumerable implementation

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		class MyEnumerator : IEnumerator<Message>
		{
			Messages m_msgs;
			bool m_valid;
			Message m_cur;

			public MyEnumerator(Messages msgs)
			{
				m_msgs = msgs;
			}

			#region IEnumerator implementation

			public bool MoveNext()
			{
				if (m_valid)
					m_msgs.Next();
				else
					m_valid = true;

				var v = m_msgs.Valid;

				if (v)
					m_cur = m_msgs.Current;

				return v;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			public Message Current
			{
				get
				{
					if (!m_valid)
						throw new Exception();

					return m_cur;
				}
			}

			#endregion

			#region IDisposable implementation

			public void Dispose()
			{
			}

			#endregion

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
		}

		[DllImport("libnotmuch")]
		static extern bool notmuch_messages_valid(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern void notmuch_messages_move_to_next(IntPtr messages);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_messages_get(IntPtr messages);
	}
}


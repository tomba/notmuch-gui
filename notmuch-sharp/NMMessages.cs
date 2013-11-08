using System;
using System.Collections.Generic;

namespace NotMuch
{
	public struct NMMessages : IEnumerable<NMMessage>
	{
		internal IntPtr Handle;

		public NMMessages(IntPtr handle)
		{
			this.Handle = handle;
		}

		public bool Valid { get { return Native.notmuch_messages_valid(this.Handle); } }

		public NMMessage Current
		{ 
			get
			{
				return new NMMessage(Native.notmuch_messages_get(this.Handle));
			}
		}

		public void Next()
		{
			Native.notmuch_messages_move_to_next(this.Handle);
		}

		#region IEnumerable implementation

		public IEnumerator<NMMessage> GetEnumerator()
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

		class MyEnumerator : IEnumerator<NMMessage>
		{
			NMMessages m_msgs;
			bool m_valid;
			NMMessage m_cur;

			public MyEnumerator(NMMessages msgs)
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

			public NMMessage Current
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
	}
}


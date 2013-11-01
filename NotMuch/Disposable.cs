using System;

namespace NotMuch
{
	public abstract class Disposable : IDisposable
	{
		protected Disposable(IntPtr ptr)
		{
			m_ptr = ptr;
		}

		internal IntPtr Handle { get { return m_ptr; } }

		protected abstract void DestroyHandle();

		protected IntPtr m_ptr;

		~Disposable ()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (m_ptr != IntPtr.Zero)
			{
				Console.WriteLine("destroy {0} {1}", this, m_ptr);
				DestroyHandle();
				m_ptr = IntPtr.Zero;
			}
		}
	}
}


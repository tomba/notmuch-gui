using System;

namespace NotMuch
{
	public abstract class DisposableBase : IDisposable
	{
		protected DisposableBase(IntPtr handle)
		{
			this.Handle = handle;
		}

		internal IntPtr Handle { get; private set; }

		protected abstract void DestroyHandle();

		~DisposableBase ()
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
			if (this.Handle != IntPtr.Zero)
			{
				//Console.WriteLine("destroy {0} {1}", this, this.Handle);
				DestroyHandle();
				this.Handle = IntPtr.Zero;
			}
		}
	}
}


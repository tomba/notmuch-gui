using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct NMThread
	{
		internal IntPtr Handle;

		internal NMThread(IntPtr handle)
		{
			Handle = handle;
		}
	}
}


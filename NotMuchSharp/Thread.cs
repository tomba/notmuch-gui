using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public struct Thread
	{
		internal IntPtr Handle;

		internal Thread(IntPtr handle)
		{
			Handle = handle;
		}
	}
}


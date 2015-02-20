using System;
using System.Runtime.InteropServices;
using GLib;

namespace NotMuchGUI
{
	public delegate void ChildProcessExited(int pid, int status, object data);

	public static class GLibHelpers
	{
		delegate void GChildWatchFunc(int pid,int status,IntPtr user_data);

		[DllImport("libglib-2.0.so", CallingConvention = CallingConvention.Cdecl)]
		static extern uint g_child_watch_add(int pid, GChildWatchFunc function, IntPtr data);

		public static int Pid(this Process proc)
		{
			var fi = typeof(Process).GetField("pid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			long pid = (long)fi.GetValue(proc);
			return (int)pid;
		}

		class Wrapper
		{
			public ChildProcessExited Callback;
			public GChildWatchFunc InternalCallback;
			public object Data;
		}

		static void OnProcessExited(int pid, int status, IntPtr data)
		{
			GCHandle gch = (GCHandle)data;
			var wrapper = (Wrapper)gch.Target;
			gch.Free();

			wrapper.Callback(pid, status, wrapper.Data);
		}

		public static void Watch(Process process, ChildProcessExited callback, object data = null)
		{
			var wrapper = new Wrapper()
			{
				Callback = callback,
				InternalCallback = OnProcessExited,
				Data = data,
			};

			var gchandle = GCHandle.Alloc(wrapper);
			uint r = g_child_watch_add(process.Pid(), wrapper.InternalCallback, (IntPtr)gchandle);
			if (r == 0)
			{
				gchandle.Free();
				throw new Exception("g_child_watch_add failed");
			}
		}
	}
}


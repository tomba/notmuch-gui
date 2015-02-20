using System;
using System.Text;

namespace NotMuchGUI
{
	public static class CmdHelpers
	{
		public static void LaunchDefaultApp(string filePath)
		{
			GLib.Process child;

			GLib.Process.SpawnAsync("/tmp", new[] { "xdg-open", filePath }, null,
				GLib.SpawnFlags.SearchPath, null, out child);
		}

		public static bool RunCmd(string cmd, string args, out string stdout, out string stderr, out int ret)
		{
			string cmdline = cmd + " " + args;
			return GLib.Process.SpawnCommandLineSync(cmdline, out stdout, out stderr, out ret);
		}

		public static bool RunNotmuch(string args, out string output)
		{
			string stdout;
			string stderr;
			int ret;

			if (RunCmd(MainClass.NotmuchExe, args, out stdout, out stderr, out ret) == false)
			{
				output = "<failed to run command>";
				return false;
			}

			if (ret != 0)
			{
				output = stderr;
				return false;
			}

			output = stdout;
			return true;
		}
	}
}


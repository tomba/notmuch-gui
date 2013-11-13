using System;
using System.Diagnostics;
using System.Text;

namespace NotMuchGUI
{
	public class CmdHelpers
	{
		public static bool RunCmd(string cmd, string args, out string stdout, out string stderr, out int ret, out string err, int maxbuf = 2048)
		{
			err = null;

			var psi = new ProcessStartInfo(cmd, args)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};

			StringBuilder errSb = new StringBuilder();
			StringBuilder stdSb = new StringBuilder();

			var lockOb = new object();
			bool killed = false;

			using (var p = new Process())
			{
				p.StartInfo = psi;

				p.ErrorDataReceived += (sender, e) =>
				{
					lock (lockOb)
					{
						if (killed)
							return;

						if (errSb.Length + e.Data.Length > maxbuf)
						{
							killed = true;

							try
							{
								p.Kill();
							}
							catch
							{
							}

							return;
						}

						errSb.AppendLine(e.Data);
					}
				};

				p.OutputDataReceived += (sender, e) =>
				{
					lock (lockOb)
					{
						if (killed)
							return;

						if (stdSb.Length + e.Data.Length > maxbuf)
						{
							killed = true;

							try
							{
								p.Kill();
							}
							catch
							{
							}

							return;
						}

						stdSb.AppendLine(e.Data);
					}
				};

				try
				{
					p.Start();
				}
				catch (Exception e)
				{
					err = e.Message;
					ret = 0;
					stdout = null;
					stderr = null;
					return false;
				}

				p.BeginOutputReadLine();
				p.BeginErrorReadLine();

				p.WaitForExit();

				if (killed)
				{
					err = "stdin/stdout buffer overflow";
					ret = 0;
					stdout = null;
					stderr = null;
					return false;
				}

				ret = p.ExitCode;
			}

			stdout = stdSb.ToString();
			stderr = errSb.ToString();

			return true;
		}

		public static bool RunNotmuch(string args, out string output)
		{
			string stdout;
			string stderr;
			int ret;
			string err;

			if (RunCmd(MainClass.NotmuchExe, args, out stdout, out stderr, out ret, out err) == false)
			{
				output = err;
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


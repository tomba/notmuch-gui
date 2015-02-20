using System;
using GLib;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.Collections.Generic;
using Mono.Unix.Native;

namespace NotMuchGUI
{
	public class TermDialog : Dialog
	{
		public static TermDialog Create()
		{
			var builder = new Builder(null, "NotMuchGUI.UI.TermDialog.ui", null);
			var dlg = new TermDialog(builder, builder.GetObject("TermDialog").Handle);
			return dlg;
		}

		[UI] readonly Button cancelButton;
		[UI] readonly TextView textview;

		IOChannel m_channelOut;
		IOChannel m_channelErr;

		System.Text.Encoding m_outEncoding;
		System.Text.Encoding m_errEncoding;

		Process m_process;

		TermDialog(Builder builder, IntPtr handle)
			: base(handle)
		{
			builder.Autoconnect(this);

			cancelButton.Clicked += OnButtonCancelClicked;

			var font = Pango.FontDescription.FromString("monospace");
			textview.OverrideFont(font);
		}

		void Append(string txt)
		{
			TextIter iter;
			iter = textview.Buffer.EndIter;
			textview.Buffer.Insert(ref iter, txt);
			GLib.Idle.Add(() =>
			{
				textview.ScrollToIter(textview.Buffer.EndIter, 0, false, 0, 0);
				return false;
			});
		}

		public void Start(string cmd, params string[] args)
		{
			this.Title = string.Format("Running {0}...", cmd);

			Append(string.Format("<running {0} {1}>\n", cmd, string.Join(" ", args)));

			try
			{
				var argv = new List<string>();
				argv.Add(cmd);
				argv.AddRange(args);

				StartInternal(argv.ToArray());
			}
			catch (Exception e)
			{
				Append(String.Format("<Process start failed: {0}>\n", e.Message));
			}
		}

		void StartInternal(string[] argv)
		{
			int stdin = Process.IgnorePipe;
			int stdout = Process.RequestPipe;
			int stderr = Process.RequestPipe;

			bool b = GLib.Process.SpawnAsyncWithPipes(null, argv, null,
				         SpawnFlags.SearchPath | SpawnFlags.DoNotReapChild, null,
				         out m_process,
				         ref stdin, ref stdout, ref stderr);

			if (b == false)
				throw new Exception("Failed to spawn process");

			GLibHelpers.Watch(m_process, OnProcessExited);

			m_channelOut = new IOChannel(stdout);
			m_channelOut.Flags |= IOFlags.Nonblock;
			m_outEncoding = System.Text.Encoding.GetEncoding(m_channelOut.Encoding);
			m_channelOut.AddWatch(0, IOCondition.In | IOCondition.Hup, OnReadStdout);

			m_channelErr = new IOChannel(stderr);
			m_channelErr.Flags |= IOFlags.Nonblock;
			m_errEncoding = System.Text.Encoding.GetEncoding(m_channelErr.Encoding);
			m_channelErr.AddWatch(0, IOCondition.In | IOCondition.Hup, OnReadStderr);
		}

		void OnProcessExited(int pid, int status, object data)
		{
			m_process.Close();
			m_process = null;

			// Run an iteration so that the stdout and stderr will get handled first
			Gtk.Application.RunIteration();

			this.Title = "Finished";

			if (Syscall.WIFEXITED(status))
			{
				status = Syscall.WEXITSTATUS(status);

				Append(String.Format("<Process ended with status: {0}>\n", status));
			}
			else if (Syscall.WIFSIGNALED(status))
			{
				var sig = Syscall.WTERMSIG(status);
				Append(String.Format("<Process killed by signal: {0}>\n", sig));
			}
			else if (Syscall.WIFSTOPPED(status))
			{
				var sig = Syscall.WSTOPSIG(status);
				Append(String.Format("<Process stopped by signal: {0}>\n", sig));
			}
			else
			{
				Append("<Process exited abnormally>\n");
			}

			cancelButton.Label = "Close";
			cancelButton.GrabFocus();
		}

		static byte[] g_inBuf = new byte[4096];

		bool OnReadStdout(IOChannel source, IOCondition condition)
		{
			if ((condition & IOCondition.In) == IOCondition.In)
			{
				ulong len;

				while (source.ReadChars(g_inBuf, out len) == IOStatus.Normal)
				{
					var txt = m_outEncoding.GetString(g_inBuf, 0, (int)len);
					Append(txt);
				}
			}

			if ((condition & IOCondition.Hup) == IOCondition.Hup)
			{
				Append("<stdout closed>\n");
				source.Dispose();
				return false;
			}

			return true;
		}

		bool OnReadStderr(IOChannel source, IOCondition condition)
		{
			if ((condition & IOCondition.In) == IOCondition.In)
			{
				ulong len;

				while (source.ReadChars(g_inBuf, out len) == IOStatus.Normal)
				{
					var txt = m_errEncoding.GetString(g_inBuf, 0, (int)len);
					Append(txt);
				}
			}

			if ((condition & IOCondition.Hup) == IOCondition.Hup)
			{
				Append("<stderr closed>\n");
				source.Dispose();
				return false;
			}

			return true;
		}

		void OnButtonCancelClicked(object sender, EventArgs e)
		{
			if (m_process != null)
			{
				this.Title = "Aborting...";
				Append("<Abort>\r\n");
				Syscall.kill(m_process.Pid(), Signum.SIGKILL);
			}
			else
			{
				Respond(ResponseType.Close);
			}
		}
	}
}

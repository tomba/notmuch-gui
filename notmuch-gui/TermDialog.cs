using System;
using System.Diagnostics;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

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

		Process m_process;

		TermDialog(Builder builder, IntPtr handle)
			: base(handle)
		{
			builder.Autoconnect(this);

			cancelButton.Clicked += OnButtonCancelClicked;
		}

		public void Start(string cmd, params string[] args)
		{
			this.Title = string.Format("Running...");

			textview.Buffer.InsertAtCursor(string.Format("Running {0} {1}\n", cmd, string.Join(" ", args)));

			var psi = new ProcessStartInfo(cmd, string.Join(" ", args))
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};

			var p = new Process();
			p.StartInfo = psi;
			p.ErrorDataReceived += (sender, e) =>
			{
				//Console.WriteLine("ERR {0}", e.Data);

				if (string.IsNullOrEmpty(e.Data))
					return;

				Application.Invoke((s, o) => textview.Buffer.InsertAtCursor(e.Data + "\n"));
			};
			p.OutputDataReceived += (sender, e) =>
			{
				//Console.WriteLine("OUT '{0}'", e.Data);

				if (string.IsNullOrEmpty(e.Data))
					return;

				Application.Invoke((s, o) => textview.Buffer.InsertAtCursor(e.Data + "\n"));
			};

			p.Exited += (sender, e) =>
			{
				//Console.WriteLine("EXIT");

				Application.Invoke((s, o) =>
				{
					this.Title = "Finished";

					textview.Buffer.InsertAtCursor("<Process ended>\n");

					cancelButton.Label = "Close";
					cancelButton.GrabFocus();

					m_process = null;
				});
			};

			m_process = p;

			try
			{
				p.Start();

				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.EnableRaisingEvents = true;
			}
			catch (Exception e)
			{
				textview.Buffer.InsertAtCursor(String.Format("<Process start failed: {0}>\n", e.Message));
				m_process = null;
			}
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			if (m_process != null)
			{
				this.Title = "Aborted";
				textview.Buffer.InsertAtCursor("<Abort>\r\n");
				m_process.Kill();
			}
			else
			{
				Respond(ResponseType.Close);
			}
		}
	}
}

using System;
using Mono.Unix.Native;
using System.Collections.Generic;

namespace NotMuchGUI
{
	public partial class TermDialog : Gtk.Dialog
	{
		//Vte.Terminal m_term;
		int m_pid = -1;

		public TermDialog()
		{
			/*
			this.Build();

			m_term = new Vte.Terminal();
			termScrolledwindow.Child = m_term;

			m_term.ChildExited += ChildExited;

			m_term.IsFocus = true;

			m_term.ShowAll();
			*/
		}

		void ChildExited(object sender, EventArgs e)
		{
			m_pid = -1;
			/*
			buttonCancel.Label = "Close";
			buttonCancel.GrabFocus();

			m_term.Feed("<Process ended>\r\n");
			m_term.Sensitive = false;*/
		}

		public void Start(string cmd, params string[] args)
		{
			var l = new List<string>();
			l.Add(System.IO.Path.GetFileName(cmd));
			l.AddRange(args);
			l.Add(null);
			/*
			m_pid = m_term.ForkCommand(cmd, l.ToArray(),
				null,
				Environment.GetEnvironmentVariable("HOME"),
				false, false, false);

			if (m_pid < 0)
			{
				m_term.Feed("<Process start failed>\r\n");
				throw new Exception("Process start failed");
			}*/
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			/*
			if (m_pid >= 0)
			{
				m_term.Feed("<Abort>\r\n");
				Syscall.kill(m_pid, Signum.SIGABRT);
			}
			else
			{
				Respond(Gtk.ResponseType.Close);
			}
			*/
		}
	}
}

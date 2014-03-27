using System;
using Mono.Unix.Native;

namespace NotMuchGUI
{
	public partial class FetchDialog : Gtk.Dialog
	{
		Vte.Terminal m_term;

		int m_pid = -1;

		public FetchDialog()
		{
			this.Build();

			m_term = new Vte.Terminal();
			termScrolledwindow.Child = m_term;

			m_term.ChildExited += ChildExited;

			m_term.IsFocus = true;

			m_term.ShowAll();
		}

		void ChildExited(object sender, EventArgs e)
		{
			m_pid = -1;

			buttonCancel.Label = "Close";

			m_term.Feed("<Process ended>\r\n");
			m_term.Sensitive = false;
		}

		public void Start()
		{
			m_pid = m_term.ForkCommand(MainClass.NotmuchExe, new string[] { "notmuch", "new", null },
				null,
				Environment.GetEnvironmentVariable("HOME"),
				false, false, false);

			if (m_pid < 0)
			{
				m_term.Feed("<Process start failed>\r\n");
				throw new Exception("Process start failed");
			}
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			if (m_pid >= 0)
			{
				m_term.Feed("<Abort>\r\n");
				Syscall.kill(m_pid, Signum.SIGABRT);
			}
			else
			{
				Respond(Gtk.ResponseType.Close);
			}
		}
	}
}

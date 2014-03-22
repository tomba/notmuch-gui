using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace NotMuchGUI
{
	public partial class RunWindow : Gtk.Window
	{
		Vte.Terminal m_term;

		public RunWindow() :
			base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			m_term = new Vte.Terminal();
			termScrolledwindow.Child = m_term;

			m_term.ChildExited += (object sender, EventArgs e) =>
			{
				m_term.Feed("<Process ended>");
				m_term.Sensitive = false;
			};

			m_term.IsFocus = true;

			m_term.Reset(true, true);
			m_term.Sensitive = true;

			m_term.ShowAll();
		}

		public void Run()
		{
			int pid = m_term.ForkCommand(MainClass.NotmuchExe, new string[] { "notmuch", "new", null },
				null,
				Environment.GetEnvironmentVariable("HOME"),
				false, false, false);

			if (pid < 0)
				m_term.Feed("<Process start failed>");
		}
	}
}

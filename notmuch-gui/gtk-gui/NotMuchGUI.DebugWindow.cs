
// This file has been generated by the GUI designer. Do not modify.
namespace NotMuchGUI
{
	public partial class DebugWindow
	{
		private global::Gtk.VBox vbox1;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TextView textviewSrc;
		private global::Gtk.ScrolledWindow GtkScrolledWindow1;
		private global::Gtk.TextView textviewDump;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget NotMuchGUI.DebugWindow
			this.Name = "NotMuchGUI.DebugWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("DebugWindow");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child NotMuchGUI.DebugWindow.Gtk.Container+ContainerChild
			this.vbox1 = new global::Gtk.VBox ();
			this.vbox1.Name = "vbox1";
			this.vbox1.Spacing = 6;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.textviewSrc = new global::Gtk.TextView ();
			this.textviewSrc.CanFocus = true;
			this.textviewSrc.Name = "textviewSrc";
			this.GtkScrolledWindow.Add (this.textviewSrc);
			this.vbox1.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.GtkScrolledWindow]));
			w2.Position = 0;
			// Container child vbox1.Gtk.Box+BoxChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.textviewDump = new global::Gtk.TextView ();
			this.textviewDump.CanFocus = true;
			this.textviewDump.Name = "textviewDump";
			this.GtkScrolledWindow1.Add (this.textviewDump);
			this.vbox1.Add (this.GtkScrolledWindow1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.vbox1 [this.GtkScrolledWindow1]));
			w4.Position = 1;
			this.Add (this.vbox1);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 575;
			this.DefaultHeight = 465;
			this.Show ();
		}
	}
}

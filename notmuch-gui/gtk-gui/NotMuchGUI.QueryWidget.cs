
// This file has been generated by the GUI designer. Do not modify.
namespace NotMuchGUI
{
	public partial class QueryWidget
	{
		private global::Gtk.VBox vbox2;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView queryTreeview;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget NotMuchGUI.QueryWidget
			global::Stetic.BinContainer.Attach (this);
			this.Name = "NotMuchGUI.QueryWidget";
			// Container child NotMuchGUI.QueryWidget.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.queryTreeview = new global::Gtk.TreeView ();
			this.queryTreeview.CanFocus = true;
			this.queryTreeview.Name = "queryTreeview";
			this.queryTreeview.EnableSearch = false;
			this.GtkScrolledWindow.Add (this.queryTreeview);
			this.vbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
			w2.Position = 0;
			this.Add (this.vbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.Hide ();
			this.queryTreeview.CursorChanged += new global::System.EventHandler (this.OnQueryTreeviewCursorChanged);
		}
	}
}
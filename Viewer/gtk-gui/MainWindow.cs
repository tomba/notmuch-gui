
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.VBox vbox2;
	private global::Gtk.HPaned hpaned1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView treeviewSearch;
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	private global::Gtk.TreeView treeviewList;
	private global::Gtk.ScrolledWindow scrolledwindowWeb;
	private global::Gtk.ScrolledWindow GtkScrolledWindow2;
	private global::Gtk.TextView textview1;
	private global::Gtk.Statusbar statusbar2;
	private global::Gtk.Label label1;
	private global::Gtk.Label label2;
	private global::Gtk.Label label3;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(4));
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox2 = new global::Gtk.VBox ();
		this.vbox2.Name = "vbox2";
		this.vbox2.Spacing = 6;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hpaned1 = new global::Gtk.HPaned ();
		this.hpaned1.CanFocus = true;
		this.hpaned1.Name = "hpaned1";
		this.hpaned1.Position = 477;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.treeviewSearch = new global::Gtk.TreeView ();
		this.treeviewSearch.CanFocus = true;
		this.treeviewSearch.Name = "treeviewSearch";
		this.GtkScrolledWindow.Add (this.treeviewSearch);
		this.hpaned1.Add (this.GtkScrolledWindow);
		global::Gtk.Paned.PanedChild w2 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.GtkScrolledWindow]));
		w2.Resize = false;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.treeviewList = new global::Gtk.TreeView ();
		this.treeviewList.CanFocus = true;
		this.treeviewList.Name = "treeviewList";
		this.GtkScrolledWindow1.Add (this.treeviewList);
		this.hpaned1.Add (this.GtkScrolledWindow1);
		this.vbox2.Add (this.hpaned1);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hpaned1]));
		w5.Position = 0;
		// Container child vbox2.Gtk.Box+BoxChild
		this.scrolledwindowWeb = new global::Gtk.ScrolledWindow ();
		this.scrolledwindowWeb.CanFocus = true;
		this.scrolledwindowWeb.Name = "scrolledwindowWeb";
		this.scrolledwindowWeb.ShadowType = ((global::Gtk.ShadowType)(1));
		this.vbox2.Add (this.scrolledwindowWeb);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.scrolledwindowWeb]));
		w6.Position = 1;
		// Container child vbox2.Gtk.Box+BoxChild
		this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
		this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
		this.textview1 = new global::Gtk.TextView ();
		this.textview1.CanFocus = true;
		this.textview1.Name = "textview1";
		this.GtkScrolledWindow2.Add (this.textview1);
		this.vbox2.Add (this.GtkScrolledWindow2);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow2]));
		w8.Position = 2;
		// Container child vbox2.Gtk.Box+BoxChild
		this.statusbar2 = new global::Gtk.Statusbar ();
		this.statusbar2.Name = "statusbar2";
		this.statusbar2.Spacing = 6;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label1 = new global::Gtk.Label ();
		this.label1.Name = "label1";
		this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
		this.statusbar2.Add (this.label1);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label1]));
		w9.Position = 1;
		w9.Expand = false;
		w9.Fill = false;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label2 = new global::Gtk.Label ();
		this.label2.Name = "label2";
		this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("label2");
		this.statusbar2.Add (this.label2);
		global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label2]));
		w10.Position = 2;
		w10.Expand = false;
		w10.Fill = false;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label3 = new global::Gtk.Label ();
		this.label3.Name = "label3";
		this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
		this.statusbar2.Add (this.label3);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label3]));
		w11.PackType = ((global::Gtk.PackType)(1));
		w11.Position = 3;
		w11.Expand = false;
		w11.Fill = false;
		this.vbox2.Add (this.statusbar2);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.statusbar2]));
		w12.Position = 3;
		w12.Expand = false;
		w12.Fill = false;
		this.Add (this.vbox2);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.DefaultWidth = 997;
		this.DefaultHeight = 646;
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.treeviewSearch.CursorChanged += new global::System.EventHandler (this.OnTreeviewSearchCursorChanged);
		this.treeviewList.CursorChanged += new global::System.EventHandler (this.OnTreeviewListCursorChanged);
	}
}

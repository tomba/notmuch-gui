
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action dialogWarningAction;
	private global::Gtk.Action editAction;
	private global::Gtk.Action replyAllAction;
	private global::Gtk.VBox vbox2;
	private global::Gtk.Toolbar toolbar1;
	private global::Gtk.Entry queryEntry;
	private global::Gtk.HPaned hpaned1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView treeviewSearch;
	private global::Gtk.ScrolledWindow GtkScrolledWindow1;
	private global::Gtk.TreeView treeviewList;
	private global::Gtk.ScrolledWindow scrolledwindowWeb;
	private global::Gtk.HBox hbox1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow2;
	private global::Gtk.TextView textviewSrc;
	private global::Gtk.ScrolledWindow GtkScrolledWindow3;
	private global::Gtk.TextView textviewDump;
	private global::Gtk.Statusbar statusbar2;
	private global::Gtk.Label label1;
	private global::Gtk.Label label2;
	private global::Gtk.Label label3;

	protected virtual void Build ()
	{
		global::Stetic.Gui.Initialize (this);
		// Widget MainWindow
		this.UIManager = new global::Gtk.UIManager ();
		global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
		this.dialogWarningAction = new global::Gtk.Action ("dialogWarningAction", global::Mono.Unix.Catalog.GetString ("GC"), "", "gtk-dialog-warning");
		this.dialogWarningAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("GC");
		w1.Add (this.dialogWarningAction, null);
		this.editAction = new global::Gtk.Action ("editAction", global::Mono.Unix.Catalog.GetString ("Reply"), null, "gtk-edit");
		this.editAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Reply");
		w1.Add (this.editAction, null);
		this.replyAllAction = new global::Gtk.Action ("replyAllAction", global::Mono.Unix.Catalog.GetString ("Reply All"), null, "gtk-edit");
		this.replyAllAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Reply All");
		w1.Add (this.replyAllAction, null);
		this.UIManager.InsertActionGroup (w1, 0);
		this.AddAccelGroup (this.UIManager.AccelGroup);
		this.Name = "MainWindow";
		this.Title = global::Mono.Unix.Catalog.GetString ("MainWindow");
		this.WindowPosition = ((global::Gtk.WindowPosition)(1));
		this.AllowShrink = true;
		this.DefaultWidth = 1300;
		this.DefaultHeight = 800;
		// Container child MainWindow.Gtk.Container+ContainerChild
		this.vbox2 = new global::Gtk.VBox ();
		this.vbox2.Name = "vbox2";
		this.vbox2.Spacing = 6;
		// Container child vbox2.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar1'><toolitem name='dialogWarningAction' action='dialogWarningAction'/><toolitem name='editAction' action='editAction'/><toolitem name='replyAllAction' action='replyAllAction'/></toolbar></ui>");
		this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
		this.toolbar1.Name = "toolbar1";
		this.toolbar1.ShowArrow = false;
		this.toolbar1.ToolbarStyle = ((global::Gtk.ToolbarStyle)(2));
		this.vbox2.Add (this.toolbar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.toolbar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.queryEntry = new global::Gtk.Entry ();
		this.queryEntry.CanFocus = true;
		this.queryEntry.Name = "queryEntry";
		this.queryEntry.IsEditable = true;
		this.queryEntry.InvisibleChar = '•';
		this.vbox2.Add (this.queryEntry);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.queryEntry]));
		w3.Position = 1;
		w3.Expand = false;
		w3.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hpaned1 = new global::Gtk.HPaned ();
		this.hpaned1.CanFocus = true;
		this.hpaned1.Name = "hpaned1";
		this.hpaned1.Position = 184;
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
		global::Gtk.Paned.PanedChild w5 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.GtkScrolledWindow]));
		w5.Resize = false;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
		this.GtkScrolledWindow1.VscrollbarPolicy = ((global::Gtk.PolicyType)(0));
		this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
		this.treeviewList = new global::Gtk.TreeView ();
		this.treeviewList.CanFocus = true;
		this.treeviewList.Name = "treeviewList";
		this.treeviewList.FixedHeightMode = true;
		this.treeviewList.Reorderable = true;
		this.GtkScrolledWindow1.Add (this.treeviewList);
		this.hpaned1.Add (this.GtkScrolledWindow1);
		this.vbox2.Add (this.hpaned1);
		global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hpaned1]));
		w8.Position = 2;
		// Container child vbox2.Gtk.Box+BoxChild
		this.scrolledwindowWeb = new global::Gtk.ScrolledWindow ();
		this.scrolledwindowWeb.CanFocus = true;
		this.scrolledwindowWeb.Name = "scrolledwindowWeb";
		this.scrolledwindowWeb.VscrollbarPolicy = ((global::Gtk.PolicyType)(0));
		this.scrolledwindowWeb.ShadowType = ((global::Gtk.ShadowType)(1));
		this.vbox2.Add (this.scrolledwindowWeb);
		global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.scrolledwindowWeb]));
		w9.Position = 3;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox1 = new global::Gtk.HBox ();
		this.hbox1.Name = "hbox1";
		this.hbox1.Spacing = 6;
		// Container child hbox1.Gtk.Box+BoxChild
		this.GtkScrolledWindow2 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow2.Name = "GtkScrolledWindow2";
		this.GtkScrolledWindow2.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow2.Gtk.Container+ContainerChild
		this.textviewSrc = new global::Gtk.TextView ();
		this.textviewSrc.CanFocus = true;
		this.textviewSrc.Name = "textviewSrc";
		this.GtkScrolledWindow2.Add (this.textviewSrc);
		this.hbox1.Add (this.GtkScrolledWindow2);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.GtkScrolledWindow2]));
		w11.Position = 0;
		// Container child hbox1.Gtk.Box+BoxChild
		this.GtkScrolledWindow3 = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow3.Name = "GtkScrolledWindow3";
		this.GtkScrolledWindow3.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow3.Gtk.Container+ContainerChild
		this.textviewDump = new global::Gtk.TextView ();
		this.textviewDump.CanFocus = true;
		this.textviewDump.Name = "textviewDump";
		this.GtkScrolledWindow3.Add (this.textviewDump);
		this.hbox1.Add (this.GtkScrolledWindow3);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.GtkScrolledWindow3]));
		w13.Position = 1;
		this.vbox2.Add (this.hbox1);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox1]));
		w14.Position = 4;
		// Container child vbox2.Gtk.Box+BoxChild
		this.statusbar2 = new global::Gtk.Statusbar ();
		this.statusbar2.Name = "statusbar2";
		this.statusbar2.Spacing = 6;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label1 = new global::Gtk.Label ();
		this.label1.Name = "label1";
		this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
		this.statusbar2.Add (this.label1);
		global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label1]));
		w15.Position = 1;
		w15.Expand = false;
		w15.Fill = false;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label2 = new global::Gtk.Label ();
		this.label2.Name = "label2";
		this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("label2");
		this.statusbar2.Add (this.label2);
		global::Gtk.Box.BoxChild w16 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label2]));
		w16.Position = 2;
		w16.Expand = false;
		w16.Fill = false;
		// Container child statusbar2.Gtk.Box+BoxChild
		this.label3 = new global::Gtk.Label ();
		this.label3.Name = "label3";
		this.label3.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
		this.statusbar2.Add (this.label3);
		global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.statusbar2 [this.label3]));
		w17.PackType = ((global::Gtk.PackType)(1));
		w17.Position = 3;
		w17.Expand = false;
		w17.Fill = false;
		this.vbox2.Add (this.statusbar2);
		global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.statusbar2]));
		w18.Position = 5;
		w18.Expand = false;
		w18.Fill = false;
		this.Add (this.vbox2);
		if ((this.Child != null)) {
			this.Child.ShowAll ();
		}
		this.Show ();
		this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
		this.dialogWarningAction.Activated += new global::System.EventHandler (this.OnGcActionActivated);
		this.editAction.Activated += new global::System.EventHandler (this.OnReplyActionActivated);
		this.replyAllAction.Activated += new global::System.EventHandler (this.OnReplyAllActionActivated);
		this.queryEntry.Changed += new global::System.EventHandler (this.OnQueryEntryChanged);
		this.queryEntry.Activated += new global::System.EventHandler (this.OnQueryEntryActivated);
		this.treeviewSearch.CursorChanged += new global::System.EventHandler (this.OnTreeviewSearchCursorChanged);
		this.treeviewList.CursorChanged += new global::System.EventHandler (this.OnTreeviewListCursorChanged);
	}
}

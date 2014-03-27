
// This file has been generated by the GUI designer. Do not modify.

public partial class MainWindow
{
	private global::Gtk.UIManager UIManager;
	private global::Gtk.Action dialogWarningAction;
	private global::Gtk.Action editAction;
	private global::Gtk.Action replyAllAction;
	private global::Gtk.ToggleAction dbgAction;
	private global::Gtk.ToggleAction threadedAction;
	private global::Gtk.Action ViewAction;
	private global::Gtk.Action refreshAction;
	private global::Gtk.ToggleAction msgSrcAction;
	private global::Gtk.Action fetchAction;
	private global::Gtk.Action DeleteAction;
	private global::Gtk.Action toggleReadAction;
	private global::Gtk.VBox vbox2;
	private global::Gtk.MenuBar menubar1;
	private global::Gtk.Toolbar toolbar1;
	private global::Gtk.HBox hbox2;
	private global::Gtk.Entry queryEntry;
	private global::Gtk.Label label7;
	private global::Gtk.Entry dateSearchEntry;
	private global::Gtk.HPaned hpaned1;
	private global::Gtk.ScrolledWindow GtkScrolledWindow;
	private global::Gtk.TreeView queryTreeview;
	private global::NotMuchGUI.MessageListWidget messageListWidget;
	private global::Gtk.HBox hbox3;
	private global::NotMuchGUI.MessageWidget messagewidget1;
	private global::NotMuchGUI.TagsWidget tagsWidget;
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
		this.dbgAction = new global::Gtk.ToggleAction ("dbgAction", global::Mono.Unix.Catalog.GetString ("Dbg"), null, null);
		this.dbgAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Dbg");
		w1.Add (this.dbgAction, null);
		this.threadedAction = new global::Gtk.ToggleAction ("threadedAction", global::Mono.Unix.Catalog.GetString ("Threaded"), null, null);
		this.threadedAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Threaded");
		w1.Add (this.threadedAction, null);
		this.ViewAction = new global::Gtk.Action ("ViewAction", global::Mono.Unix.Catalog.GetString ("View"), null, null);
		this.ViewAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("View");
		w1.Add (this.ViewAction, null);
		this.refreshAction = new global::Gtk.Action ("refreshAction", global::Mono.Unix.Catalog.GetString ("_Refresh"), null, "gtk-refresh");
		this.refreshAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_Refresh");
		w1.Add (this.refreshAction, "F5");
		this.msgSrcAction = new global::Gtk.ToggleAction ("msgSrcAction", global::Mono.Unix.Catalog.GetString ("Msg Src"), null, null);
		this.msgSrcAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Msg Src");
		w1.Add (this.msgSrcAction, null);
		this.fetchAction = new global::Gtk.Action ("fetchAction", global::Mono.Unix.Catalog.GetString ("Fetch"), null, null);
		this.fetchAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Fetch");
		w1.Add (this.fetchAction, null);
		this.DeleteAction = new global::Gtk.Action ("DeleteAction", global::Mono.Unix.Catalog.GetString ("Delete"), null, null);
		this.DeleteAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Delete");
		w1.Add (this.DeleteAction, "Delete");
		this.toggleReadAction = new global::Gtk.Action ("toggleReadAction", global::Mono.Unix.Catalog.GetString ("Toggle Read"), null, null);
		this.toggleReadAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Toggle Read");
		w1.Add (this.toggleReadAction, "m");
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
		this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='ViewAction' action='ViewAction'><menuitem name='refreshAction' action='refreshAction'/><menuitem name='DeleteAction' action='DeleteAction'/><menuitem name='toggleReadAction' action='toggleReadAction'/></menu></menubar></ui>");
		this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
		this.menubar1.Name = "menubar1";
		this.vbox2.Add (this.menubar1);
		global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.menubar1]));
		w2.Position = 0;
		w2.Expand = false;
		w2.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.UIManager.AddUiFromString ("<ui><toolbar name='toolbar1'><toolitem name='dialogWarningAction' action='dialogWarningAction'/><toolitem name='editAction' action='editAction'/><toolitem name='replyAllAction' action='replyAllAction'/><toolitem name='dbgAction' action='dbgAction'/><toolitem name='threadedAction' action='threadedAction'/><toolitem name='msgSrcAction' action='msgSrcAction'/><toolitem name='fetchAction' action='fetchAction'/><toolitem name='DeleteAction' action='DeleteAction'/><toolitem name='toggleReadAction' action='toggleReadAction'/></toolbar></ui>");
		this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget ("/toolbar1")));
		this.toolbar1.Name = "toolbar1";
		this.toolbar1.ShowArrow = false;
		this.toolbar1.ToolbarStyle = ((global::Gtk.ToolbarStyle)(1));
		this.vbox2.Add (this.toolbar1);
		global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.toolbar1]));
		w3.Position = 1;
		w3.Expand = false;
		w3.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox2 = new global::Gtk.HBox ();
		this.hbox2.Name = "hbox2";
		this.hbox2.Spacing = 6;
		// Container child hbox2.Gtk.Box+BoxChild
		this.queryEntry = new global::Gtk.Entry ();
		this.queryEntry.CanFocus = true;
		this.queryEntry.Name = "queryEntry";
		this.queryEntry.IsEditable = true;
		this.queryEntry.InvisibleChar = '•';
		this.hbox2.Add (this.queryEntry);
		global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.queryEntry]));
		w4.Position = 0;
		// Container child hbox2.Gtk.Box+BoxChild
		this.label7 = new global::Gtk.Label ();
		this.label7.Name = "label7";
		this.label7.LabelProp = global::Mono.Unix.Catalog.GetString ("Date");
		this.hbox2.Add (this.label7);
		global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.label7]));
		w5.Position = 1;
		w5.Expand = false;
		w5.Fill = false;
		// Container child hbox2.Gtk.Box+BoxChild
		this.dateSearchEntry = new global::Gtk.Entry ();
		this.dateSearchEntry.CanFocus = true;
		this.dateSearchEntry.Name = "dateSearchEntry";
		this.dateSearchEntry.IsEditable = true;
		this.dateSearchEntry.InvisibleChar = '•';
		this.hbox2.Add (this.dateSearchEntry);
		global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.dateSearchEntry]));
		w6.Position = 2;
		this.vbox2.Add (this.hbox2);
		global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
		w7.Position = 2;
		w7.Expand = false;
		w7.Fill = false;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hpaned1 = new global::Gtk.HPaned ();
		this.hpaned1.CanFocus = true;
		this.hpaned1.Name = "hpaned1";
		this.hpaned1.Position = 353;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
		this.GtkScrolledWindow.Name = "GtkScrolledWindow";
		this.GtkScrolledWindow.HscrollbarPolicy = ((global::Gtk.PolicyType)(2));
		this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
		// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
		this.queryTreeview = new global::Gtk.TreeView ();
		this.queryTreeview.CanFocus = true;
		this.queryTreeview.Name = "queryTreeview";
		this.queryTreeview.EnableSearch = false;
		this.GtkScrolledWindow.Add (this.queryTreeview);
		this.hpaned1.Add (this.GtkScrolledWindow);
		global::Gtk.Paned.PanedChild w9 = ((global::Gtk.Paned.PanedChild)(this.hpaned1 [this.GtkScrolledWindow]));
		w9.Resize = false;
		// Container child hpaned1.Gtk.Paned+PanedChild
		this.messageListWidget = new global::NotMuchGUI.MessageListWidget ();
		this.messageListWidget.Events = ((global::Gdk.EventMask)(256));
		this.messageListWidget.Name = "messageListWidget";
		this.messageListWidget.ThreadedView = false;
		this.hpaned1.Add (this.messageListWidget);
		this.vbox2.Add (this.hpaned1);
		global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hpaned1]));
		w11.Position = 3;
		// Container child vbox2.Gtk.Box+BoxChild
		this.hbox3 = new global::Gtk.HBox ();
		this.hbox3.Name = "hbox3";
		this.hbox3.Spacing = 6;
		// Container child hbox3.Gtk.Box+BoxChild
		this.messagewidget1 = new global::NotMuchGUI.MessageWidget ();
		this.messagewidget1.Events = ((global::Gdk.EventMask)(256));
		this.messagewidget1.Name = "messagewidget1";
		this.messagewidget1.ShowHtmlSource = false;
		this.hbox3.Add (this.messagewidget1);
		global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.messagewidget1]));
		w12.Position = 0;
		// Container child hbox3.Gtk.Box+BoxChild
		this.tagsWidget = new global::NotMuchGUI.TagsWidget ();
		this.tagsWidget.Events = ((global::Gdk.EventMask)(256));
		this.tagsWidget.Name = "tagsWidget";
		this.hbox3.Add (this.tagsWidget);
		global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox3 [this.tagsWidget]));
		w13.PackType = ((global::Gtk.PackType)(1));
		w13.Position = 1;
		w13.Expand = false;
		w13.Fill = false;
		this.vbox2.Add (this.hbox3);
		global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox3]));
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
		w15.Position = 0;
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
		w18.PackType = ((global::Gtk.PackType)(1));
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
		this.dbgAction.Activated += new global::System.EventHandler (this.OnDbgActionActivated);
		this.threadedAction.Activated += new global::System.EventHandler (this.OnThreadedActionActivated);
		this.refreshAction.Activated += new global::System.EventHandler (this.OnRefreshActionActivated);
		this.msgSrcAction.Activated += new global::System.EventHandler (this.OnMsgSrcActionActivated);
		this.fetchAction.Activated += new global::System.EventHandler (this.OnFetchActionActivated);
		this.DeleteAction.Activated += new global::System.EventHandler (this.OnDeleteActionActivated);
		this.toggleReadAction.Activated += new global::System.EventHandler (this.OnToggleReadActionActivated);
		this.queryEntry.Changed += new global::System.EventHandler (this.OnQueryEntryChanged);
		this.queryEntry.Activated += new global::System.EventHandler (this.OnQueryEntryActivated);
		this.dateSearchEntry.Activated += new global::System.EventHandler (this.OnDateSearchEntryActivated);
		this.queryTreeview.CursorChanged += new global::System.EventHandler (this.OnQueryTreeviewCursorChanged);
		this.messageListWidget.MessageSelected += new global::System.EventHandler (this.OnMessageListWidgetMessageSelected);
		this.messageListWidget.CountsChanged += new global::System.EventHandler (this.OnMessageListWidgetCountsChanged);
	}
}

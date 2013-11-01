using System;
using Gtk;
using NotMuch;
using System.IO;

public partial class MainWindow: Gtk.Window
{
	Database m_db;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		SetupQueryList();
		SetupMailList();

		var path = "/home/tomba/tmp/nm-db";
		m_db = Database.Open(path, DatabaseMode.READ_ONLY);
	}

	protected override void OnDestroyed()
	{
		base.OnDestroyed();

		m_db.Dispose();
		m_db = null;
	}

	void SetupQueryList()
	{
		// Create a column for the artist name
		var queryColumn = new Gtk.TreeViewColumn();
		queryColumn.Title = "Query";

		// Create the text cell that will display the artist name
		var queryCell = new Gtk.CellRendererText();

		// Add the cell to the column
		queryColumn.PackStart(queryCell, true);

		treeviewSearch.AppendColumn(queryColumn);

		queryColumn.AddAttribute(queryCell, "text", 0);

		var queryStore = new Gtk.ListStore(typeof(string));

		treeviewSearch.Model = queryStore;

		queryStore.AppendValues("Tomi");
		queryStore.AppendValues("from:ti.com");
		queryStore.AppendValues("Test3");
	}

	void SetupMailList()
	{
		treeviewList.AppendColumn("From", new Gtk.CellRendererText(), "text", 0);
		treeviewList.AppendColumn("Subject", new Gtk.CellRendererText(), "text", 1);

		m_mailStore = new Gtk.ListStore(typeof(string), typeof(string), typeof(string));
		treeviewList.Model = m_mailStore;
	}

	Gtk.ListStore m_mailStore;

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnTreeviewSearchRowActivated(object o, RowActivatedArgs args)
	{
	}

	protected void OnTreeviewSearchCursorChanged(object sender, EventArgs e)
	{
		m_mailStore.Clear();

		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		// THE ITER WILL POINT TO THE SELECTED ROW
		if (selection.GetSelected(out model, out iter))
		{
			//Console.WriteLine("Selected Value:" + model.GetValue(iter, 0).ToString() + model.GetValue(iter, 1).ToString());

			var queryString = model.GetValue(iter, 0).ToString();

			var q = Query.Create(m_db, queryString);

			var msgs = q.Search();

			foreach (var msg in msgs)
			{
				var filename = msg.FileName;
				var from = msg.GetHeader("From");
				var subject = msg.GetHeader("Subject");

				m_mailStore.AppendValues(from, subject, filename);
			}

			q.Dispose();
		}
	}

	protected void OnTreeviewListCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		// THE ITER WILL POINT TO THE SELECTED ROW
		if (selection.GetSelected(out model, out iter))
		{
			//Console.WriteLine("Selected Value:" + model.GetValue(iter, 0).ToString() + model.GetValue(iter, 1).ToString());

			var filename = model.GetValue(iter, 2).ToString();

			var text = File.ReadAllText(filename);

			textview1.Buffer.Text = text;
		}
		else
		{
			textview1.Buffer.Clear();
		}
	}
}

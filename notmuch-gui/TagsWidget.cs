using System;
using Gtk;
using System.Collections.Generic;
using NM = NotMuch;
using System.Linq;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
	public partial class TagsWidget : Gtk.Box
	{
		ListStore m_tagStore;
		List<string> m_tags = new List<string>();
		string m_msgId;

		[UI] Gtk.TreeView tagsTreeview;

		public TagsWidget(Gtk.Builder builder, IntPtr handle) : base(handle)
		{
			builder.Autoconnect(this);

			var c = new TreeViewColumn();
			c.Title = "Tags";

			var crToggle = new Gtk.CellRendererToggle();
			crToggle.Toggled += OnTagToggled;
			c.PackStart(crToggle, false);
			c.AddAttribute(crToggle, "active", 1);

			var crText = new CellRendererText();
			crText.Mode = CellRendererMode.Activatable;
			c.PackStart(crText, false);
			c.AddAttribute(crText, "text", 0);

			tagsTreeview.AppendColumn(c);

			m_tagStore = new ListStore(typeof(string), typeof(bool));
			tagsTreeview.Model = m_tagStore;
		}

		void OnTagToggled(object sender, ToggledArgs args)
		{
			TreeIter iter;

			if (m_tagStore.GetIter(out iter, new TreePath(args.Path)))
			{
				bool old = (bool)m_tagStore.GetValue(iter, 1);
				m_tagStore.SetValue(iter, 1, !old);
			}
		}

		public void UpdateTagsView(NM.Message msg, List<string> allTags)
		{
			m_msgId = msg.ID;

			m_tags.Clear();
			m_tags.AddRange(msg.GetTags());

			m_tagStore.Clear();

			foreach (var t in m_tags)
				m_tagStore.AppendValues(t, true);

			foreach (var t in allTags.Except(m_tags))
				m_tagStore.AppendValues(t, false);
		}

		// XXX
		void OnApplyButtonClicked(object sender, EventArgs e)
		{
			var selectedList = new List<string>();

			foreach (object[] item in m_tagStore)
			{
				bool selected = (bool)item[1];

				if (selected)
				{
					string tag = (string)item[0];
					selectedList.Add(tag);
				}
			}

			var addTags = selectedList.Except(m_tags);
			var rmTags = m_tags.Except(selectedList);

			NM.Status stat;
			using (var db = NM.Database.Open(MainClass.DatabasePath, NM.DatabaseMode.READ_WRITE, out stat))
			{
				if (stat != NM.Status.SUCCESS)
					throw new Exception();

				var m = db.FindMessage(m_msgId);

				using (var atomic = db.BeginAtomic())
				{
					foreach (var tag in addTags)
					{
						stat = m.AddTag(tag);
						Console.WriteLine("Added tag {0}: {1}", tag, stat);
					}

					foreach (var tag in rmTags)
					{
						stat = m.RemoveTag(tag);
						Console.WriteLine("Removed tag {0}: {1}", tag, stat);
					}
				}
			}

			m_tags = selectedList;
		}

		// XXX
		void OnResetButtonClicked(object sender, EventArgs e)
		{
		}
	}
}


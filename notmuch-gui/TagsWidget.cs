using System;
using Gtk;
using System.Collections.Generic;
using NM = NotMuch;
using System.Linq;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TagsWidget : Gtk.Bin
	{
		ListStore m_tagStore;
		NM.Message m_msg;

		public TagsWidget()
		{
			this.Build();

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

		List<string> m_tags = new List<string>();

		public void UpdateTagsView(NM.Message msg, List<string> allTags)
		{
			m_msg = msg;

			m_tags.Clear();

			var tags = msg.GetTags();

			while (tags.Valid)
			{
				m_tags.Add(tags.Current);
				tags.Next();
			}

			m_tagStore.Clear();

			foreach (var t in m_tags)
				m_tagStore.AppendValues(t, true);

			foreach (var t in allTags.Except(m_tags))
				m_tagStore.AppendValues(t, false);
		}

		protected void OnApplyButtonClicked(object sender, EventArgs e)
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

			var da2 = m_msg.Date;

			NM.Status stat;
			using (var db = NM.Database.Open(MainClass.DatabasePath, NM.DatabaseMode.READ_WRITE, out stat))
			{
				if (stat != NM.Status.SUCCESS)
					throw new Exception();

				var m = db.FindMessage(m_msg.Id);

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

			m_tags = selectedList;
		}

		protected void OnResetButtonClicked(object sender, EventArgs e)
		{
		}
	}
}


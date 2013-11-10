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

		public void UpdateTagsView(NM.Message msg, List<string> allTags)
		{
			m_tagStore.Clear();

			var tags = msg.GetTags();

			List<string> selList = new List<string>();

			while (tags.Valid)
			{
				selList.Add(tags.Current);
				tags.Next();
			}

			foreach (var t in selList)
				m_tagStore.AppendValues(t, true);

			foreach (var t in allTags.Except(selList))
				m_tagStore.AppendValues(t, false);
		}
	}
}


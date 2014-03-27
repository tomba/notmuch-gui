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
		ListStore m_tagsStore;
		string[] m_tags = new string[0];

		// msgid -> tags[]
		Dictionary<string, string[]> m_idTagsMap;

		public event Action<IEnumerable<string>> MsgTagsUpdatedEvent;

		public TagsWidget()
		{
			this.Build();

			var c = new TreeViewColumn();
			c.Title = "Tags";

			var crToggle = new Gtk.CellRendererToggle();
			crToggle.Toggled += OnTagToggled;
			c.PackStart(crToggle, false);
			c.AddAttribute(crToggle, "active", 1);
			c.AddAttribute(crToggle, "inconsistent", 2);

			var crText = new CellRendererText();
			crText.Mode = CellRendererMode.Activatable;
			c.PackStart(crText, false);
			c.AddAttribute(crText, "text", 0);

			tagsTreeview.AppendColumn(c);

			m_tagsStore = new ListStore(typeof(string), typeof(bool), typeof(bool));
			tagsTreeview.Model = m_tagsStore;
		}

		public void SetDBTags(IEnumerable<string> tags)
		{
			m_tags = tags.ToArray();
		}

		void OnTagToggled(object sender, ToggledArgs args)
		{
			TreeIter iter;

			if (m_tagsStore.GetIter(out iter, new TreePath(args.Path)))
			{
				bool old = (bool)m_tagsStore.GetValue(iter, 1);
				m_tagsStore.SetValue(iter, 1, !old);
				m_tagsStore.SetValue(iter, 2, false);
			}

			applyButton.Sensitive = true;
		}

		public void Clear()
		{
			m_idTagsMap = null;
			m_tagsStore.Clear();
			this.Sensitive = false;
		}

		public void UpdateTagsView(string[] ids)
		{
			if (ids.Length == 0)
			{
				Clear();
				return;
			}

			this.Sensitive = true;
			applyButton.Sensitive = false;

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				m_idTagsMap = ids.ToDictionary(id => id, id => db.GetMessage(id).GetTags().ToArray());
			}

			var allTags = m_idTagsMap.Values.SelectMany(v => v);

			/* add new tags to m_allDBTags */
			var missingTags = allTags.Except(m_tags).ToArray();
			if (missingTags.Length > 0)
			{
				m_tags = m_tags.Concat(missingTags).ToArray();
				Array.Sort(m_tags);
			}

			var tagCounts = m_tags.Select(t => new { Tag = t, Count = allTags.Count(arg => arg == t) });

			int numMsgs = ids.Length;

			m_tagsStore.Clear();

			foreach (var tagCount in tagCounts)
			{
				if (tagCount.Count == numMsgs)
					m_tagsStore.AppendValues(tagCount.Tag, true, false);
				else if (tagCount.Count > 0)
					m_tagsStore.AppendValues(tagCount.Tag, false, true);
				else
					m_tagsStore.AppendValues(tagCount.Tag, false, false);
			}
		}

		protected void OnApplyButtonClicked(object sender, EventArgs e)
		{
			if (m_idTagsMap == null)
				return;

			var inconsistentList = new List<string>();
			var selectedList = new List<string>();

			foreach (object[] item in m_tagsStore)
			{
				string tag = (string)item[0];
				bool selected = (bool)item[1];
				bool inconsistent = (bool)item[2];

				if (inconsistent)
					inconsistentList.Add(tag);
				else if (selected)
					selectedList.Add(tag);
			}

			using (var cdb = new CachedDB(true))
			{
				var db = cdb.Database;

				foreach (var kvp in m_idTagsMap)
				{
					var id = kvp.Key;
					var origTags = kvp.Value;

					var addTags = selectedList.Except(origTags);
					var rmTags = origTags.Except(selectedList).Except(inconsistentList);

					var m = db.GetMessage(id);

					using (var atomic = db.BeginAtomic())
					{
						foreach (var tag in addTags)
						{
							var stat = m.AddTag(tag);
							Console.WriteLine("{0}: Added tag {1}: {2}", id, tag, stat);
						}

						foreach (var tag in rmTags)
						{
							var stat = m.RemoveTag(tag);
							Console.WriteLine("{0}: Removed tag {1}: {2}", id, tag, stat);
						}
					}
				}
			}

			MsgTagsUpdatedEvent(m_idTagsMap.Keys);

			UpdateTagsView(m_idTagsMap.Keys.ToArray());
		}

		protected void OnResetButtonClicked(object sender, EventArgs e)
		{
			if (m_idTagsMap == null)
				return;

			UpdateTagsView(m_idTagsMap.Keys.ToArray());
		}
	}
}


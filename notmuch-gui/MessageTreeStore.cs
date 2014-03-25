using System;
using Gtk;
using System.Collections.Generic;

namespace NotMuchGUI
{
	class MessageTreeStore : TreeStore
	{
		public MessageTreeStore() :
			base(typeof(string), typeof(string), typeof(string), typeof(long), typeof(string),
			     typeof(int), typeof(int), typeof(int), typeof(int))
		{
		}

		public MessageListFlags GetFlags(ref TreeIter iter)
		{
			return (MessageListFlags)GetValue(iter, (int)MessageListColumns.Flags);
		}

		public int GetThreadNum(ref TreeIter iter)
		{
			return (int)GetValue(iter, (int)MessageListColumns.ThreadNum);
		}

		public long GetDate(ref TreeIter iter)
		{
			return (long)GetValue(iter, (int)MessageListColumns.Date);
		}

		public string GetMessageID(ref TreeIter iter)
		{
			return (string)GetValue(iter, (int)MessageListColumns.MessageId);
		}

		public void SetTags(ref TreeIter iter, List<string> tags)
		{
			SetValue(iter, (int)MessageListColumns.Tags, string.Join("/", tags));
		}

		public void SetFlags(ref TreeIter iter, MessageListFlags flags)
		{
			SetValue(iter, (int)MessageListColumns.Flags, (int)flags);
		}

		public void SetValues(ref TreeIter iter, string msgId, string from, string subject,
		                      long dateStamp, List<string> tags, MessageListFlags flags,
		                      int depth, int msgNum, int threadNum)
		{
			SetValues(iter,
				msgId,
				from,
				subject,
				dateStamp,
				string.Join("/", tags),
				(int)flags,
				depth,
				msgNum,
				threadNum);
		}
	}
}

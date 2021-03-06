using System;
using System.Collections.Generic;
using Gtk;
using NM = NotMuch;
using System.Linq;

namespace NotMuchGUI
{
	class MessageTreeStore : TreeStore
	{
		public MessageTreeStore() :
			base(typeof(string), typeof(string), typeof(string), typeof(long), typeof(string[]),
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

		public string[] GetTags(ref TreeIter iter)
		{
			return (string[])GetValue(iter, (int)MessageListColumns.Tags);
		}

		public void SetValues(ref TreeIter iter, NM.Message msg, MessageListFlags flags,
		                      int depth, int msgNum, int threadNum)
		{
			var tags = msg.GetTags().ToArray();

			if (tags.Contains("unread"))
				flags |= MessageListFlags.Unread;
			else
				flags &= ~MessageListFlags.Unread;

			if (tags.Contains("deleted"))
				flags |= MessageListFlags.Deleted;
			else
				flags &= ~MessageListFlags.Deleted;

			SetValues(iter,
				msg.ID,
				msg.From,
				msg.Subject,
				msg.DateStamp,
				tags,
				(int)flags,
				depth,
				msgNum,
				threadNum);
		}

		public void UpdateValues(ref TreeIter iter, NM.Message msg)
		{
			var flags = GetFlags(ref iter);

			var tags = msg.GetTags().ToArray();

			if (tags.Contains("unread"))
				flags |= MessageListFlags.Unread;
			else
				flags &= ~MessageListFlags.Unread;

			if (tags.Contains("deleted"))
				flags |= MessageListFlags.Deleted;
			else
				flags &= ~MessageListFlags.Deleted;

			SetValues(iter,
				msg.ID,
				msg.From,
				msg.Subject,
				msg.DateStamp,
				tags,
				(int)flags);
		}
	}
}

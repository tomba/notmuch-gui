using System;

namespace NotMuchGUI
{
	enum MessageListColumns
	{
		MessageId,
		From,
		Subject,
		Date,
		Tags,
		Flags,
		Depth,
		MsgNum,
		ThreadNum,
		NUM_COLUMNS,
	}

	[Flags]
	enum MessageListFlags
	{
		None = 0,
		Unread = 1 << 0,
		NoMatch = 1 << 1,
		Excluded = 1 << 2,
		Deleted = 1 << 3,
	}
}


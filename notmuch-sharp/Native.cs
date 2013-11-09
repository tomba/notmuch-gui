using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	unsafe static class Native
	{
		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_create(string path, out IntPtr db);

		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_open(string path, DatabaseMode mode, out IntPtr db);

		[DllImport("libnotmuch")]
		public static extern void notmuch_database_destroy(IntPtr db);

		[DllImport("libnotmuch")]
		public static extern void notmuch_database_close(IntPtr db);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_database_get_path(IntPtr db);

		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_find_message(IntPtr db, string messageId, out IntPtr msg);


		// Query
		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_query_create(IntPtr db, string queryString);

		[DllImport("libnotmuch")]
		public static extern void notmuch_query_destroy(IntPtr query);

		[DllImport("libnotmuch")]
		public static extern uint notmuch_query_count_messages(IntPtr query);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_query_search_messages(IntPtr query);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_query_search_threads(IntPtr query);


		// Messages
		[DllImport("libnotmuch")]
		public static extern bool notmuch_messages_valid(IntPtr messages);

		[DllImport("libnotmuch")]
		public static extern void notmuch_messages_move_to_next(IntPtr messages);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_messages_get(IntPtr messages);


		// Message
		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_destroy(IntPtr message);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_get_message_id(IntPtr message);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_get_date(IntPtr message);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_get_filename(IntPtr message);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_get_header(IntPtr message, string header);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_message_get_tags(IntPtr message);


		// Threads
		[DllImport("libnotmuch")]
		public static extern bool notmuch_threads_valid(IntPtr threads);

		[DllImport("libnotmuch")]
		public static extern void notmuch_threads_move_to_next(IntPtr threads);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_threads_get(IntPtr threads);


		// Tags
		[DllImport("libnotmuch")]
		public static extern bool notmuch_tags_valid(IntPtr tags);

		[DllImport("libnotmuch")]
		public static extern void notmuch_tags_move_to_next(IntPtr tags);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_tags_get(IntPtr tags);
	}

	public enum DatabaseMode
	{
		READ_ONLY = 0,
		READ_WRITE
	}

	enum Status
	{
		SUCCESS = 0,
		OUT_OF_MEMORY,
		READ_ONLY_DATABASE,
		XAPIAN_EXCEPTION,
		FILE_ERROR,
		FILE_NOT_EMAIL,
		DUPLICATE_MESSAGE_ID,
		NULL_POINTER,
		TAG_TOO_LONG,
		UNBALANCED_FREEZE_THAW,
		UNBALANCED_ATOMIC,
		LAST_STATUS
	}
}

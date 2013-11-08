using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	unsafe static class Native
	{
		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_create(string path, out IntPtr dbPtr);

		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_open(string path, DatabaseMode mode, out IntPtr dbPtr);

		[DllImport("libnotmuch")]
		public static extern void notmuch_database_destroy(IntPtr ptr);

		[DllImport("libnotmuch")]
		public static extern void notmuch_database_close(IntPtr ptr);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_database_get_path(IntPtr ptr);

		[DllImport("libnotmuch")]
		public static extern Status notmuch_database_find_message(IntPtr ptr, string messageId, out IntPtr msgPtr);

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

		// Threads
		[DllImport("libnotmuch")]
		public static extern bool notmuch_threads_valid(IntPtr threads);

		[DllImport("libnotmuch")]
		public static extern void notmuch_threads_move_to_next(IntPtr threads);

		[DllImport("libnotmuch")]
		public static extern IntPtr notmuch_threads_get(IntPtr threads);

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

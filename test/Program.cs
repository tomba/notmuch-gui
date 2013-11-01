using System;
using System.IO;
using NotMuch;

namespace test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			bool create = false;

			Database db;

			var path = "/home/tomba/tmp/nm-db";

			if (create)
			{
				if (Directory.Exists(path + "/.notmuch"))
					new DirectoryInfo(path + "/.notmuch").Delete(true);

				db = Database.Create(path);
			}
			else
			{
				db = Database.Open(path, DatabaseMode.READ_ONLY);
			}

			Console.WriteLine("path {0}", db.Path);

			var q = Query.Create(db, "tomi");

			q.Run();

			q.Dispose();

			db.Dispose();
		}
	}
}

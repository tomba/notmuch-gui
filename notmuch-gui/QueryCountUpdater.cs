using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

namespace NotMuchGUI
{
	class QueryCountUpdater
	{
		public event Action<string, int, bool> QueryCountCalculated;

		Task m_task;
		volatile bool m_cancel;

		public QueryCountUpdater()
		{
		}

		public void Start(string[] queries)
		{
			if (m_task != null)
				Cancel();

			m_task = Task.Factory.StartNew(() =>
			{
				UpdateQueryCounts(queries);
				m_task = null;
			});
		}

		public void Cancel()
		{
			if (m_task == null)
				return;

			m_cancel = true;
			m_task.Wait();
			m_task = null;
			m_cancel = false;
		}

		void UpdateQueryCounts(string[] queries)
		{
			var sw = Stopwatch.StartNew();

			var list = queries.SelectMany(q => new []
			{
				new { Key = q, Query = q, Unread = false },
				new { Key = q, Query = (q != "" && q != "*") ? q + " AND tag:unread" : "tag:unread", Unread = true }
			});

			Parallel.ForEach(list,
				() =>
				{
					var cdb = new CachedDB();
					return cdb;
				},
				(val, state, cdb) =>
				{
					if (state.IsStopped)
					{
						//Console.WriteLine("{0} stopped, exit", Thread.CurrentThread.ManagedThreadId);
						return cdb;
					}

					if (m_cancel)
					{
						//Console.WriteLine("{0} set stop", Thread.CurrentThread.ManagedThreadId);
						state.Stop();
						return cdb;
					}

					var db = cdb.Database;

					using (var query = db.CreateQuery(val.Query))
					{
						int count = query.CountMessages();

						GLib.Idle.Add(() =>
						{
							if (this.QueryCountCalculated != null)
								this.QueryCountCalculated(val.Key, count, val.Unread);

							return false;
						});
					}

					return cdb;
				},
				(cdb) =>
				{
					//Console.WriteLine("{0} ended", Thread.CurrentThread.ManagedThreadId);
					cdb.Dispose();
				}
			);

			sw.Stop();

			Console.WriteLine("Updated query counts in {0} ms", sw.ElapsedMilliseconds);
		}
	}
}


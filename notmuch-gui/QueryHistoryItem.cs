using System;

namespace NotMuchGUI
{
	public class QueryHistoryItem : IEquatable<QueryHistoryItem>
	{
		public string Query { get; set; }
		public bool Threaded { get; set; }

		public QueryHistoryItem()
		{
		}

		#region IEquatable implementation

		public bool Equals(QueryHistoryItem other)
		{
			return this.Query == other.Query && this.Threaded == other.Threaded;
		}

		#endregion
	}
}

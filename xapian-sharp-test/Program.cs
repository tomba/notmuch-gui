using System;
using Xapian;

namespace test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var db = new Xapian.Database("/home/tomba/Maildir/.notmuch/xapian");

			var query_string = "to:tomba@iki.fi";

			// Parse the query string to produce a Xapian::Query object.

			var qp = new Xapian.QueryParser();
			qp.SetStemmer(new Xapian.Stem("english"));
			qp.SetDatabase(db);
			qp.SetDefaultOp(Query.op.OP_AND);
			qp.SetStemmingStrategy(Xapian.QueryParser.stem_strategy.STEM_SOME);
			qp.AddPrefix("to", "XTO");

			var query = qp.ParseQuery(query_string,
				            (int)(Xapian.QueryParser.feature_flag.FLAG_BOOLEAN
				            | QueryParser.feature_flag.FLAG_PHRASE
				            | QueryParser.feature_flag.FLAG_LOVEHATE
				            | QueryParser.feature_flag.FLAG_BOOLEAN_ANY_CASE
				            | QueryParser.feature_flag.FLAG_WILDCARD
				            | QueryParser.feature_flag.FLAG_PURE_NOT)
			            );

			Console.WriteLine("Parsed query is: " + query.GetDescription());

			// Find the top 10 results for the query.
			var enquire = new Xapian.Enquire(db);
			enquire.SetQuery(query);
			Xapian.MSet matches = enquire.GetMSet(0, 10);

			// Display the results.
			Console.WriteLine("{0} results found.", matches.GetMatchesEstimated());
			Console.WriteLine("Matches 1-{0}:", matches.Size());

			Xapian.MSetIterator m = matches.Begin();
			while (m != matches.End())
			{
				Console.WriteLine("{0}: {1}% docid={2} [{3}]\n",
					m.GetRank() + 1,
					m.GetPercent(),
					m.GetDocId(),
					m.GetDocument().GetValue(1));
				//m.GetDocument().GetData());
				++m;
			}
		}
	}
}

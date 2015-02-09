using System;
using System.Text;
using System.IO;

namespace NotMuchGUI
{
	public static class TextToHtmlHelper
	{
		public static bool UseBlockQuotes = true;

		public static string TextToHtml(string text)
		{
			var sb = new StringBuilder();

			using (StringReader reader = new StringReader(text))
			{
				string line;

				int currentDepth = 0;

				while ((line = reader.ReadLine()) != null)
				{
					if (UseBlockQuotes)
					{
						int depth;
						line = StripCitations(line, out depth);

						while (currentDepth < depth)
						{
							sb.AppendLine("<blockquote>");
							currentDepth++;
						}

						while (currentDepth > depth)
						{
							sb.AppendLine("</blockquote>");
							currentDepth--;
						}
					}
					else
					{
						int depth = CitationDepth(line);

						if (depth > 0 && currentDepth == 0)
							sb.AppendLine("<font color=888888>");
						else if (depth == 0 && currentDepth > 0)
							sb.AppendLine("</font>");

						currentDepth = depth;
					}

					sb.Append(System.Net.WebUtility.HtmlEncode(line));

					sb.AppendLine();
					sb.AppendLine("<br>");
				}

				if (UseBlockQuotes)
				{
					while (currentDepth > 0)
					{
						sb.AppendLine("</blockquote>");
						currentDepth--;
					}
				}
				else
				{
					if (currentDepth > 0)
						sb.AppendLine("</font>");
				}
			}

			return sb.ToString();
		}

		static int CitationDepth(string line)
		{
			int depth = 0;

			for (int i = 0; i < line.Length; ++i)
			{
				if (line[i] == ' ')
					continue;

				if (line[i] == '>')
				{
					depth++;
					continue;
				}

				break;
			}

			return depth;
		}

		static string StripCitations(string line, out int depth)
		{
			depth = 0;

			for (int i = 0; i < line.Length; ++i)
			{
				if (line[i] == ' ')
					continue;

				if (line[i] == '>')
				{
					depth++;
					continue;
				}

				return line.Substring(i);
			}

			return "";
		}
	}
}


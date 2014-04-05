using System;
using System.Text;
using System.IO;

namespace NotMuchGUI
{
	public static class TextToHtmlHelper
	{
		public static string TextToHtml(string text)
		{
			var sb = new StringBuilder();

			sb.AppendLine("<pre>");

			using (StringReader reader = new StringReader(text))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					int depth = CitationDepth(line);

					if (depth > 0)
						sb.AppendFormat("<font color={0}>", "888888");

					sb.Append(line);

					if (depth > 0)
						sb.Append("</font>");

					sb.AppendLine();
				}
			}

			sb.AppendLine("</pre>");

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
	}
}


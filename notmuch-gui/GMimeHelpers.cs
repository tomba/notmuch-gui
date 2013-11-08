using System;
using GMime;

namespace NotMuchGUI
{
	enum GMimeHtmlFilterFlags
	{
		PRE = 1 << 0,
		CONVERT_NL = 1 << 1,
		CONVERT_SPACES = 1 << 2,
		CONVERT_URLS = 1 << 3,
		MARK_CITATION = 1 << 4,
		CONVERT_ADDRESSES = 1 << 5,
		ESCAPE_8BIT = 1 << 6,
		CITE = 1 << 7,
	}

	static class GMimeHelpers
	{
		public static void DumpStructure(Entity ent)
		{
			if (ent is Message)
			{
				var msg = (Message)ent;

				Console.WriteLine("{0}", ent.GetType());

				DumpStructure(msg.MimePart);
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				Console.WriteLine("{0}", ent.GetType());

				foreach (Entity part in mp)
					DumpStructure(part);
			}
			else if (ent is Part)
			{
				var part = (Part)ent;
				Console.WriteLine("{0}: {1}, {2}, {3}",
					part.GetType(), part.ContentType.ToString(), part.ContentType.GetParameter("charset"),
					part.ContentEncoding.ToString());
			}
			else
			{
				throw new Exception();
			}
		}

		public static Part FindFirstContent(Entity ent, ContentType ct)
		{
			if (ent is Message)
			{
				var msg = (Message)ent;

				return FindFirstContent(msg.MimePart, ct);
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				foreach (Entity part in mp)
				{
					var p = FindFirstContent(part, ct);
					if (p != null)
						return p;
				}

				return null;
			}
			else if (ent is Part)
			{
				var part = (Part)ent;

				if (part.ContentType.IsType(ct.MediaType, ct.MediaSubtype))
					return part;
				else
					return null;
			}
			else
			{
				throw new Exception();
			}
		}
	}
}


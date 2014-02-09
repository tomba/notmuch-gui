using System;
using GMime;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
		public static void DumpStructure(Entity ent, StringWriter sw, int indent)
		{
			const int step = 4;
			string indentStr = new string(' ', indent);

			sw.Write("{0}{1}", indentStr, ent.GetType());

			if (ent.ContentType != null)
				sw.Write(" ContentType({0}, {1}), ContentID({2})",
					ent.ContentType.ToString(), ent.ContentType.GetParameter("charset"),
					ent.ContentId);

			if (ent.ContentId != null)
				sw.Write(" ContentId({0})", ent.ContentId);

			if (ent.ContentDisposition != null)
				sw.Write(" ContentDisposition({0}, {1})", ent.ContentDisposition.Disposition,
					ent.ContentDisposition.GetParameter("filename"));

			if (ent is Message)
			{
				var msg = (Message)ent;

				sw.WriteLine();

				DumpStructure(msg.MimePart, sw, indent + step);
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				sw.WriteLine();

				foreach (Entity part in mp)
					DumpStructure(part, sw, indent + step);
			}
			else if (ent is MessagePart)
			{
				var mp = (MessagePart)ent;

				sw.WriteLine();

				DumpStructure(mp.Message, sw, indent + step);
			}
			else if (ent is Part)
			{
				var part = (Part)ent;
				sw.WriteLine(" ContentEncoding({0})", part.ContentEncoding.ToString());
			}
			else
			{
				throw new Exception();
			}
		}

		public static IEnumerable<Entity> GetAllEntities(Entity ent)
		{
			yield return ent;

			if (ent is Message)
			{
				var msg = (Message)ent;

				foreach (var p in GetAllEntities(msg.MimePart))
					yield return p;
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				foreach (Entity part in mp)
					foreach (var p in GetAllEntities(part))
						yield return p;
			}
			else if (ent is MessagePart)
			{
				var mp = (MessagePart)ent;

				foreach (var p in GetAllEntities(mp.Message))
					yield return p;
			}
			else if (ent is Part)
			{
			}
			else
			{
				throw new Exception();
			}
		}

		public static Part FindFirstContent(Entity ent, ContentType ct)
		{
			return GetAllEntities(ent)
				.OfType<Part>()
				.FirstOrDefault(p => p.ContentType.IsType(ct.MediaType, ct.MediaSubtype));
		}

		public static IEnumerable<Part> GetAttachments(Entity entity)
		{
			return GetAllEntities(entity)
				.OfType<Part>()
				.Where(p => p.ContentDisposition != null && p.ContentDisposition.Disposition == "attachment");
		}
	}
}


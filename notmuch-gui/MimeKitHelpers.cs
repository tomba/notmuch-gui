using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MimeKit;

namespace NotMuchGUI
{
	static class MimeKitHelpers
	{
		const int step = 4;

		public static void DumpMessage(MimeMessage msg, StringBuilder sb, int indent)
		{
			sb.AppendFormat("{0}\n", msg.GetType());
			DumpEntity(msg.Body, sb, indent + step);
		}

		static void DumpEntity(MimeEntity ent, StringBuilder sb, int indent)
		{
			string indentStr = new string(' ', indent);

			sb.AppendFormat("{0}{1}", indentStr, ent.GetType());

			if (ent.ContentType != null)
				sb.AppendFormat(" ContentType({0}, {1}), ContentID({2})",
					ent.ContentType.ToString(), ent.ContentType.Charset, ent.ContentId);

			if (ent.ContentId != null)
				sb.AppendFormat(" ContentId({0})", ent.ContentId);

			if (ent.ContentDisposition != null)
				sb.AppendFormat(" ContentDisposition({0}, {1})", ent.ContentDisposition.Disposition,
					ent.ContentDisposition.FileName);

			if (ent is MessagePart)
			{
				var mp = (MessagePart)ent;

				sb.AppendLine();

				DumpMessage(mp.Message, sb, indent + step);
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				sb.AppendLine();

				foreach (var part in mp)
					DumpEntity(part, sb, indent + step);
			}
			else // MimePart
			{
				var part = (MimePart)ent;
				sb.AppendFormat(" ContentEncoding({0})\n", part.ContentTransferEncoding);
			}
		}

		public static IEnumerable<object> GetAllEntities(object ent)
		{
			yield return ent;

			if (ent is MimeMessage)
			{
				var msg = (MimeMessage)ent;

				foreach (var p in GetAllEntities(msg.Body))
					yield return p;
			}
			else if (ent is Multipart)
			{
				var mp = (Multipart)ent;

				foreach (MimeEntity part in mp)
					foreach (var p in GetAllEntities(part))
						yield return p;
			}
			else if (ent is MessagePart)
			{
				var mp = (MessagePart)ent;

				foreach (var p in GetAllEntities(mp.Message))
					yield return p;
			}
			else if (ent is MimePart)
			{
			}
			else
			{
				throw new Exception();
			}
		}

		public static TextPart FindFirstContent(MimeMessage ent, ContentType ct)
		{
			return GetAllEntities(ent)
					.OfType<TextPart>()
					.FirstOrDefault(p => p.ContentType.Matches(ct.MediaType, ct.MediaSubtype));
		}
	}
}


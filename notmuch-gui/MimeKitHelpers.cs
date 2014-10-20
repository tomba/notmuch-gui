using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MimeKit;
using System.IO;

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

		public static MimeMessage CreateReply(MimeMessage msg, bool replyAll)
		{
			var reply = new MimeMessage();

			reply.InReplyTo = msg.MessageId;

			foreach (var r in msg.References)
				reply.References.Add(r);

			reply.References.Add(msg.MessageId);

			reply.Subject = "Re: " + msg.Subject;

			reply.From.Add(Globals.MyAddresses.First());	// XXX
			reply.To.AddRange(msg.From);

			if (replyAll)
			{
				var comparer = new AddressComparer();

				reply.To.AddRange(msg.To.Except(Globals.MyAddresses, comparer));

				var cc = msg.Cc
					.Except(Globals.MyAddresses, comparer)
					.Except(reply.To, comparer).ToArray();

				if (cc.Length > 0)
					reply.Cc.AddRange(cc);
			}

			var textpart = msg.BodyParts.OfType<TextPart>()
				.FirstOrDefault(p => p.ContentType.Matches("text", "plain"));

			var sb = new StringBuilder();

			sb.AppendFormat("On {1}, {0} wrote:\n\n",
				msg.From.First().Name,
				msg.Date.ToString("u"));

			using (StringReader reader = new StringReader(textpart.Text))
			{
				string prefix = "> ";
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					sb.Append(prefix);
					sb.AppendLine(line);
				}
			}

			reply.Body = new TextPart("plain")
			{
				Text = sb.ToString(),
			};

			return reply;
		}
	}
}
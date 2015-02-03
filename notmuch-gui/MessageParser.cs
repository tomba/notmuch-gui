using System;
using System.Linq;
using System.Collections.Generic;
using MimeKit;
using MimeKit.Cryptography;
using System.Text;

namespace NotMuchGUI
{
	public class MessageParser
	{
		public class MessagePiece
		{
			public MimePart MimePart;
			public string Text;
		}

		public class Attachment
		{
			public MimePart MimePart;
		}

		public class ParseContext
		{
			public List<MessagePiece> Pieces = new List<MessagePiece>();
			public List<Attachment> Attachments = new List<Attachment>();

			public void AddMimePart(MimePart mimePart)
			{
				this.Pieces.Add(new MessagePiece()
				{
					MimePart = mimePart,
				});
			}

			public void AddText(string text)
			{
				this.Pieces.Add(new MessagePiece()
				{
					Text = text,
				});
			}

			public void AddAttachment(MimePart mimePart)
			{
				this.Attachments.Add(new Attachment()
				{
					MimePart = mimePart,
				});
			}
		}

		public static void ParseMessage(MimeEntity ent, ParseContext ctx)
		{
			if (ent is MessagePart)
			{
				//var mp = (MessagePart)ent;
				throw new NotImplementedException();
			}
			else if (ent is Multipart)
			{
				var multiPart = (Multipart)ent;

				switch (ent.ContentType.MediaSubtype)
				{
				case "mixed":
					{
						foreach (var part in multiPart)
							ParseMessage(part, ctx);
					}
					break;

				case "alternative":
					{
						var body = multiPart.Last();
						ParseMessage(body, ctx);
					}
					break;

				case "encrypted":
					{
						var encryptedPart = (MultipartEncrypted)multiPart;

						var gpgCtx = new MyGPGContext();

						try
						{
							var decrypted = encryptedPart.Decrypt(gpgCtx);

							var sb = new StringBuilder();
							sb.AppendLine("<small>");
							sb.AppendLine("-- Start encrypted part --<br>");
							sb.AppendLine("</small>");
							ctx.AddText(sb.ToString());

							ParseMessage(decrypted, ctx);

							ctx.AddText("<small>-- End encrypted part --</small><p>");
						}
						catch (OperationCanceledException)
						{
							ctx.AddText("Canceled");
						}
						catch (UnauthorizedAccessException)
						{
							ctx.AddText("Unauthorized");
						}
						catch (PrivateKeyNotFoundException)
						{
							ctx.AddText("Private key not found");
						}
						catch (Exception e)
						{
							ctx.AddText(String.Format("Error: {0}", e.Message));
						}
					}
					break;

				case "signed":
					{
						var signedPart = (MultipartSigned)multiPart;

						var gpgCtx = new MyGPGContext();

						var sb = new StringBuilder();
						sb.AppendLine("<small>");
						sb.AppendLine("-- Start signed part --<br>");

						var sigs = signedPart.Verify(gpgCtx);

						foreach (var signature in sigs)
						{
							try
							{
								bool valid = signature.Verify();

								var cert = signature.SignerCertificate;

								var txt = String.Format("{0} &lt;{1}&gt; ({2})",
									          cert.Name, cert.Email, cert.Fingerprint);

								if (valid)
									sb.AppendLine("Signed by " + txt + "<br>");
								else
									sb.AppendLine("FAILED to verify signature by " + txt + "<br>");
							}
							catch (DigitalSignatureVerifyException e)
							{
								sb.AppendLine(String.Format("Error: {0}<br>", e.Message));
							}
						}

						sb.AppendLine("</small>");

						ctx.AddText(sb.ToString());

						ParseMessage(multiPart[0], ctx);

						ctx.AddText("<small>-- End signed part --</small><p>");
					}
					break;

				default:
					ctx.AddText(String.Format("unhandled multipart type {0}", ent.ContentType.MediaSubtype));
					break;
				}
			}
			else if (ent is MimePart)
			{
				var mimepart = (MimePart)ent;

				if (mimepart is ApplicationPgpSignature)
				{
					return;
				}

				if (mimepart.ContentDisposition != null && mimepart.ContentDisposition.IsAttachment)
				{
					ctx.AddAttachment(mimepart);
					return;
				}

				ctx.AddMimePart(mimepart);
			}
			else
			{
				throw new Exception();
			}
		}

		public static ParseContext ParseMessage(MimeMessage msg)
		{
			var ctx = new ParseContext();
			ParseMessage(msg.Body, ctx);
			return ctx;
		}
	}
}

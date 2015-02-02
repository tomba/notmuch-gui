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
			public string Error;
		}

		public class Attachment
		{
			public MimePart MimePart;
		}

		public class ParseContext
		{
			public List<MessagePiece> Pieces = new List<MessagePiece>();
			public List<Attachment> Attachments = new List<Attachment>();
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
				var mp = (Multipart)ent;

				switch (ent.ContentType.MediaSubtype)
				{
				case "mixed":
					{
						foreach (var part in mp)
							ParseMessage(part, ctx);
					}
					break;

				case "alternative":
					{
						var body = mp.Last();
						ParseMessage(body, ctx);
					}
					break;

				case "encrypted":
					{
						var encryptedPart = (MultipartEncrypted)mp;

						var gpgCtx = new MyGPGContext();

						try
						{
							var decrypted = encryptedPart.Decrypt(gpgCtx);

							ParseMessage(decrypted, ctx);
						}
						catch (OperationCanceledException)
						{
							ctx.Pieces.Add(new MessagePiece() { Error = "Canceled" });
						}
						catch (UnauthorizedAccessException)
						{
							ctx.Pieces.Add(new MessagePiece() { Error = "Unauthorized" });
						}
						catch (PrivateKeyNotFoundException)
						{
							ctx.Pieces.Add(new MessagePiece() { Error = "Private key not found" });
						}
						catch (Exception e)
						{
							ctx.Pieces.Add(new MessagePiece() { Error = String.Format("Error: {0}", e.Message) });
						}
					}
					break;

				case "signed":
					{
						var signedPart = (MultipartSigned)mp;

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

						ctx.Pieces.Add(new MessagePiece() { Error = sb.ToString() });

						var body = mp.First();
						ParseMessage(body, ctx);

						ctx.Pieces.Add(new MessagePiece() { Error = "<small>-- End signed part --</small><p>" });
					}
					break;

				default:
					ctx.Pieces.Add(new MessagePiece()
					{
						Error = String.Format("unhandled multipart type {0}", ent.ContentType.MediaSubtype),
					});
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
					var attachment = new Attachment()
					{
						MimePart = mimepart,
					};

					ctx.Attachments.Add(attachment);
					return;
				}

				var msgPart = new MessagePiece()
				{
					MimePart = mimepart,
				};

				ctx.Pieces.Add(msgPart);
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

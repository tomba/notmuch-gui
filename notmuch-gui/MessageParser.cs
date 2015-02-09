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
		public class ParseContext
		{
			public StringBuilder Builder = new StringBuilder();
			public List<MimePart> Attachments = new List<MimePart>();

			public void AddMimePart(MimePart mimePart)
			{
				var textpart = (TextPart)mimePart;

				string html;

				if (textpart.ContentType.Matches("text", "html"))
					html = textpart.Text;
				else
					html = TextToHtmlHelper.TextToHtml(textpart.Text);

				this.Builder.Append(html);
			}

			public void AddLine(string text)
			{
				this.Builder.AppendLine(text);
			}

			public void AddAttachment(MimePart mimePart)
			{
				this.Attachments.Add(mimePart);
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

						ctx.AddLine("<div class=\"encrypted-part\">");

						try
						{
							DigitalSignatureCollection sigs;

							var decrypted = encryptedPart.Decrypt(gpgCtx, out sigs);

							ctx.AddLine("<div class=\"encrypted-header\">");

							foreach (var signature in sigs)
							{
								try
								{
									bool valid = signature.Verify();

									var cert = signature.SignerCertificate;

									var txt = String.Format("{0} &lt;{1}&gt; ({2})",
										cert.Name, cert.Email, cert.Fingerprint);

									if (valid)
										ctx.AddLine("Encrypted & Signed by " + txt + "<br>");
									else
										ctx.AddLine("FAILED to verify signature by " + txt + "<br>");
								}
								catch (DigitalSignatureVerifyException e)
								{
									ctx.AddLine(String.Format("Error: {0}<br>", e.Message));
								}
							}

							ctx.AddLine("</div>");  /* encrypted-header */

							ParseMessage(decrypted, ctx);
						}
						catch (OperationCanceledException)
						{
							ctx.AddLine("Canceled");
						}
						catch (UnauthorizedAccessException)
						{
							ctx.AddLine("Unauthorized");
						}
						catch (PrivateKeyNotFoundException)
						{
							ctx.AddLine("Private key not found");
						}
						catch (Exception e)
						{
							ctx.AddLine(String.Format("Error: {0}", e.Message));
						}

						ctx.AddLine("</div>");  /* encrypted-part */
					}
					break;

				case "signed":
					{
						var signedPart = (MultipartSigned)multiPart;

						var gpgCtx = new MyGPGContext();

						ctx.AddLine("<div class=\"signed-part\">");

						ctx.AddLine("<div class=\"signed-header\">");

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
									ctx.AddLine("Signed by " + txt + "<br>");
								else
									ctx.AddLine("FAILED to verify signature by " + txt + "<br>");
							}
							catch (DigitalSignatureVerifyException e)
							{
								ctx.AddLine(String.Format("Error: {0}<br>", e.Message));
							}
						}

						ctx.AddLine("</div>");  /* signed-header */

						ctx.AddLine("<div class=\"signed-content\">");
						ParseMessage(multiPart[0], ctx);
						ctx.AddLine("</div>");  /* signed-content */

						ctx.AddLine("</div>");  /* signed-part */
					}
					break;

				default:
					ctx.AddLine(String.Format("unhandled multipart type {0}", ent.ContentType.MediaSubtype));
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

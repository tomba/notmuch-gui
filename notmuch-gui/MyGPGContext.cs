using System;
using Gtk;
using MimeKit.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace NotMuchGUI
{
	class MyGPGContext : GnuPGContext
	{
		protected override string GetPasswordForKey(PgpSecretKey key)
		{
			string password;

			password = PasswordCache.Cache.GetPassword(key.KeyId);

			if (password != null)
				return password;

			var dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
				ButtonsType.OkCancel, "Enter password for key {0:X}", key.KeyId);

			dlg.Title = "Enter password";
			dlg.DefaultResponse = ResponseType.Ok;

			var content = dlg.ContentArea;
			var entry = new Entry()
			{
				ActivatesDefault = true,
				Text = "",
				Visibility = false,
				InvisibleChar = '*',
			};
			content.PackEnd(entry, true, true, 0);

			dlg.ShowAll();
			var resp = (ResponseType)dlg.Run();
			password = entry.Text;
			dlg.Destroy();

			if (resp != ResponseType.Ok)
				return null;

			try
			{
				var privateKey = key.ExtractPrivateKey(password.ToCharArray());

				if (privateKey != null)
					PasswordCache.Cache.SetPassword(key.KeyId, password);
			}
			catch (Exception)
			{
			}

			return password;
		}
	}
}


using System;
using Gtk;
using MimeKit.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Collections.Generic;

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

			var pbundle = this.PublicKeyRingBundle;
			var pkr = pbundle.GetPublicKeyRing(key.KeyId);
			var pubKey = pkr.GetPublicKey();

			List<string> uids = new List<string>();
			foreach (string uid in pubKey.GetUserIds())
				uids.Add(uid);

			var dlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Question,
				          ButtonsType.OkCancel, false,
				          "Enter passphrase for key {0:X} (master {1:X})\n{2}",
				          key.KeyId, pubKey.KeyId,
				          string.Join("\n", uids));

			dlg.Title = "Enter passphrase";
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


using System;
using System.Collections.Generic;
using MK = MimeKit;

namespace NotMuchGUI
{
	class AddressComparer : EqualityComparer<MK.InternetAddress>
	{
		#region implemented abstract members of EqualityComparer
		public override int GetHashCode(MK.InternetAddress ia)
		{
			var mba = ia as MK.MailboxAddress;
			if (mba == null)
				return ia.GetHashCode();
			return mba.Address.GetHashCode();
		}

		public override bool Equals(MK.InternetAddress x, MK.InternetAddress y)
		{
			var mba1 = x as MK.MailboxAddress;
			var mba2 = y as MK.MailboxAddress;
			if (mba1 == null || mba2 == null)
				return x.Equals(y);

			return mba1.Address == mba2.Address;
		}
		#endregion
	}
}

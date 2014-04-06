using System;
using MK = MimeKit;

namespace NotMuchGUI
{
	public static class Globals
	{
		public static readonly MK.MailboxAddress[] MyAddresses = new MK.MailboxAddress[]
		{
			new MK.MailboxAddress("Tomi Valkeinen", "tomba@iki.fi"),
			new MK.MailboxAddress("Tomi Valkeinen", "tomi.valkeinen@iki.fi"),
			new MK.MailboxAddress("Tomi Valkeinen", "tomi.valkeinen@ti.com"),
		};
	}
}


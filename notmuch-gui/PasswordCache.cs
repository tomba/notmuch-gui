using System;
using System.Collections.Generic;

namespace NotMuchGUI
{
	class PasswordCache
	{
		public static readonly PasswordCache Cache = new PasswordCache();

		readonly Dictionary<long, string> m_map = new Dictionary<long, string>();

		public void SetPassword(long keyId, string password)
		{
			m_map[keyId] = password;
		}

		public string GetPassword(long keyId)
		{
			string password;
			if (m_map.TryGetValue(keyId, out password))
				return password;
			else
				return null;
		}
	}
}

using System;
using IniParser;
using IniParser.Model;

namespace NotMuchGUI
{
	class MyKeyFile
	{
		FileIniDataParser m_parser;
		IniData m_data;

		public MyKeyFile(string filename)
		{
			m_parser = new IniParser.FileIniDataParser();
			m_data = m_parser.ReadFile(filename);
		}

		public string GetStringOrNull(string group_name, string key)
		{
			if (!m_data.Sections.ContainsSection(group_name) || !m_data.Sections[group_name].ContainsKey(key))
				return null;

			return m_data[group_name][key];
		}

		public string[] GetStringListOrNull(string group_name, string key)
		{
			if (!m_data.Sections.ContainsSection(group_name) || !m_data.Sections[group_name].ContainsKey(key))
				return null;

			var str = m_data[group_name][key];

			return str.Split(new []{ ';' }, StringSplitOptions.None);
		}

		public bool GetIntegerOrFalse(string group_name, string key, out int value)
		{
			if (!m_data.Sections.ContainsSection(group_name) || !m_data.Sections[group_name].ContainsKey(key))
			{
				value = 0;
				return false;
			}

			var str = m_data[group_name][key];

			value = Int32.Parse(str);
			return true;
		}
	}
}
	
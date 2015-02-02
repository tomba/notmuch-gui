using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NotMuchGUI
{
	class MyKeyFile
	{
		readonly Section[] m_data;

		public IEnumerable<Section> Sections { get { return m_data; } }

		public class Section
		{
			public readonly string Name;

			public IEnumerable<KeyValuePair<string, string>> KeyValues { get { return m_values; } }

			KeyValuePair<string, string>[] m_values;

			public Section(string name, IEnumerable<KeyValuePair<string, string>> keyValues)
			{
				this.Name = name;
				m_values = keyValues.ToArray();
			}

			public string FindFirstValue(string keyName)
			{
				foreach (var kvp in m_values)
				{
					if (kvp.Key == keyName)
						return kvp.Value;
				}

				return null;
			}
		}

		public MyKeyFile(string filename)
		{
			var sectionRegex = new Regex(@"^ \[ ([^\]]*) \]", RegexOptions.IgnorePatternWhitespace);
			var keyValueRegex = new Regex(@"^([^=]+) = (.*)", RegexOptions.IgnorePatternWhitespace);

			List<Section> sectionList = new List<Section>();

			string currentSection = null;
			List<KeyValuePair<string, string>> keyValueList = null;

			foreach (var line in File.ReadLines(filename))
			{
				var str = line.Trim();

				if (str.Length == 0)
					continue;

				if (str.StartsWith("#"))
					continue;

				var match = sectionRegex.Match(str);

				if (match.Success)
				{
					if (keyValueList != null)
						sectionList.Add(new Section(currentSection, keyValueList));

					currentSection = match.Groups[1].Value.Trim();
					keyValueList = new List<KeyValuePair<string, string>>();

					continue;
				}

				match = keyValueRegex.Match(str);

				if (match.Success)
				{
					if (keyValueList == null)
						throw new Exception("Key value pair before section");

					var k = match.Groups[1].Value.Trim();
					var v = match.Groups[2].Value.Trim();

					keyValueList.Add(new KeyValuePair<string, string>(k, v));

					continue;
				}

				throw new Exception("bad line in config file");
			}

			if (keyValueList != null)
				sectionList.Add(new Section(currentSection, keyValueList));

			m_data = sectionList.ToArray();
		}

		string GetFirstValue(string sectionName, string keyName)
		{
			var section = m_data.FirstOrDefault(s => s.Name == sectionName);
			if (section == null)
				return null;

			return section.FindFirstValue(keyName);
		}

		public string GetStringOrNull(string section, string key)
		{
			return GetFirstValue(section, key);
		}

		public string[] GetStringListOrNull(string section, string key)
		{
			var str = GetFirstValue(section, key);
			if (str == null)
				return null;

			return str.Split(new []{ ';' }, StringSplitOptions.None);
		}

		public bool GetIntegerOrFalse(string section, string key, out int value)
		{
			var str = GetFirstValue(section, key);
			if (str == null)
			{
				value = 0;
				return false;
			}

			value = Int32.Parse(str);
			return true;
		}
	}
}
	
using System;

namespace NotMuchGUI
{
	public static class KeyFileExtensions
	{
		public static string GetStringOrNull(this KeyFile.GKeyFile file, string group_name, string key)
		{
			if (MainClass.AppKeyFile.HasGroup(group_name) && MainClass.AppKeyFile.HasKey(group_name, key))
				return file.GetString(group_name, key);
			else
				return null;
		}

		public static string[] GetStringListOrNull(this KeyFile.GKeyFile file, string group_name, string key)
		{
			if (MainClass.AppKeyFile.HasGroup(group_name) && MainClass.AppKeyFile.HasKey(group_name, key))
				return file.GetStringList(group_name, key);
			else
				return null;
		}

		public static bool GetIntegerOrFalse(this KeyFile.GKeyFile file, string group_name, string key, out int value)
		{
			if (MainClass.AppKeyFile.HasGroup(group_name) && MainClass.AppKeyFile.HasKey(group_name, key))
			{
				value = file.GetInteger(group_name, key);
				return true;
			}
			else
			{
				value = 0;
				return false;
			}
		}
	}
}


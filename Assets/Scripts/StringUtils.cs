﻿static class StringUtils
{
	public static string ReplaceFirst(string text, string search, string replace)
	{
		int pos = text.IndexOf(search);
		if (pos < 0)
		{
			return text;
		}
		return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
	}
}
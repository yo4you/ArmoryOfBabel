/// <summary>
/// usefull methods when dealing with string operations
/// </summary>
internal static class StringUtils
{
	/// <summary>
	/// returns a string where a pattern is replaced only once according to the provided paramaters
	/// </summary>
	/// <param name="text">the starting string</param>
	/// <param name="search">the text to replace</param>
	/// <param name="replace">the replacement</param>
	/// <returns>the resulting text</returns>
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

using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// usefull methods when dealing with string operations
/// </summary>
internal static class StringUtils
{
	/// <summary>
	/// returns true if <paramref name="rh"/> matches the generalized pattern described in <paramref name="lh"/>
	/// </summary>
	/// <param name="lh"></param>
	/// <param name="rh"></param>
	/// <returns></returns>
	public static bool CompareGeneralizedString(string lh, string rh)
	{
		string[] validCharacters = SplitGeneralization(lh);
		return validCharacters.Contains(rh);
	}

	public static bool ContainsGeneralization(string text, string lh)
	{
		string[] validCharacters = SplitGeneralization(lh);
		return validCharacters.Any((c) => text.Contains(c));
	}

	public static string GeneralizedReplaceFirst(string text, string lh, string rh)
	{
		string[] validCharacters = SplitGeneralization(lh);
		foreach (var pattern in validCharacters)
		{
			int pos = text.IndexOf(pattern);
			if (pos < 0)
			{
				continue;
			}
			return text.Substring(0, pos) + rh + text.Substring(pos + pattern.Length);
		}
		return text;
	}

	/// <summary>
	/// returns a string where a pattern is replaced only once according to the provided parameters
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

	public static string[] SplitGeneralization(string lh)
	{
		return Regex.Split(lh, @"\|");
	}
}

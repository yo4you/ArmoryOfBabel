using System;

[Serializable]
public struct StringGrammarRule
{
	// the weighted chance this rule will apply over other rules
	public int Chance;

	// the characters to be replaced by this grammar rule
	public string LeftHand;

	// the character to be output by this grammar rule
	public string RightHand;
}
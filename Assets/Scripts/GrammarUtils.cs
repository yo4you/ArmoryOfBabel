using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GrammarUtils
{
	public static string TestGrammar(ref List<StringGrammarRule> grammars, string testString, int seed)
	{
		int cycle_count = 999;
		var state = Random.state;
		Random.InitState(seed);

		var total_weight = grammars.Sum(i => i.Chance);
	string_changed:
		if (cycle_count-- < 0)
		{
			Debug.Log("grammar exceeded 1000 cycles, likely cyclic");
			goto end;
		}

		var uncheckedGrammars = new List<StringGrammarRule>(grammars);
		var weight = total_weight;
	no_hit:
		if (weight <= 0)
		{
			goto end;
		}
		var randWeight = Random.Range(0, weight);

		foreach (var grammar in uncheckedGrammars)
		{
			randWeight -= grammar.Chance;
			if (randWeight < 0)
			{
				if (testString.Contains(grammar.LeftHand))
				{
					testString = StringUtils.ReplaceFirst(testString, grammar.LeftHand, grammar.RightHand);
					goto string_changed;
				}
				else
				{
					weight -= grammar.Chance;
					uncheckedGrammars.Remove(grammar);
					goto no_hit;
				}
			}
		}
	end:
		Random.state = state;
		return testString;
	}
}
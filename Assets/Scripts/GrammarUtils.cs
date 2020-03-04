using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// this class contains implementation for formal grammar execution
/// </summary>
public static class GrammarUtils
{
	/// <summary>
	/// applies a set of formal grammar rules to string and returns the result
	/// runs until no more grammar can be applied to the string or execution exceeds 1000 cycles
	/// grammars are applied in a random order according to their weighted randomness
	/// each cycle applies at most one grammar rules to the string 
	/// </summary>
	/// <param name="grammars">list of grammars to apply </param>
	/// <param name="start_string">the string to which the grammars will be applied</param>
	/// <param name="seed">random seed whith which the execute the grammars</param>
	/// <returns></returns>
	public static string ApplyGrammars(ref List<StringGrammarRule> grammars, string start_string, int seed)
	{
		// the amount of cycles we want to run before determining the grammars are too cyclic to reach a conclusion
		int cycle_count = 999;
		// store the previous random state so we can restore it later
		var random_state = Random.state;
		Random.InitState(seed);
		
		// we sum up the combined random weight of all our grammars, this is used so we determine the individual chances as portions of this total 
		var total_weight = grammars.Sum(i => i.Chance);
	string_changed:
		if (cycle_count-- < 0)
		{
			Debug.Log("grammar exceeded 1000 cycles, likely cyclic");
			goto end;
		}
		// copy of the grammar list, we'll remove inapplicable grammars from this and restore the whole list in the next cycle
		var uncheckedGrammars = new List<StringGrammarRule>(grammars);
		var weight = total_weight;
	no_hit:
		// if our weight is 0 we've exhausted all our grammars and there's no need to cycle anymore
		if (weight <= 0)
		{
			goto end;
		}
		var randWeight = Random.Range(0, weight);
		foreach (var grammar in uncheckedGrammars)
		{
			randWeight -= grammar.Chance;
			// once our weighted random is between the current element and the next we have a hit
			if (randWeight < 0)
			{
				// check if the grammar is applicable to the string
				if (start_string.Contains(grammar.LeftHand))
				{
					start_string = StringUtils.ReplaceFirst(start_string, grammar.LeftHand, grammar.RightHand);
					// apply the grammar and reset the cycle
					goto string_changed;
				}
				else
				{
					// the grammar isn't applicable we remove it and generate a new weighted random without it
					weight -= grammar.Chance;
					uncheckedGrammars.Remove(grammar);
					goto no_hit;
				}
			}
		}
	end:
		// restore the random state
		Random.state = random_state;
		return start_string;
	}

	public static NodeGraph ApplyNodeGrammars(string outputString, List<NodeGrammar> nodeGrammars, NodeGraph nodeGraph)
	{
		NodeGraph output = (NodeGraph)nodeGraph.Clone();

		return output;
	}
}
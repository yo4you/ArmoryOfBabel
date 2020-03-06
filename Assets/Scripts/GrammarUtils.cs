using mattmc3.Common.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
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

	public static NodeGraph ApplyNodeGrammars(string ruleSequence, ref List<NodeGrammar> nodeGrammars, NodeGraph nodeGraph)
	{
		NodeGraph output = (NodeGraph)nodeGraph.Clone();
		for (int i = 0; i < ruleSequence.Length; i++)
		{
			string rule_name = ruleSequence[i].ToString();
			var rules = nodeGrammars.Where((NodeGrammar gram) => gram.Name == rule_name);
			if (rules.Count() == 0)
			{
				continue;
			}
			foreach (var rule in rules)
			{
				if (IsNodeRoleApplicable(rule, ref nodeGraph))
				{
					ApplyNodeRule(rule, ref nodeGraph);
					break;
				}
			}

		}
		return output;
	}

	private static void ApplyNodeRule(NodeGrammar rule, ref NodeGraph nodeGraph)
	{
		Debug.Log("applied");
	}

	private static bool IsNodeRoleApplicable(NodeGrammar rule, ref NodeGraph nodeGraph)
	{
		var match_dict = rule.LeftHand._nodeDict;
		var smallest_index = match_dict.Keys.Min();
		var first_node = match_dict[smallest_index].Node_text;

		foreach (var match in nodeGraph._nodeDict)
		{
			if (MatchNodeGraphInsert(smallest_index, match.Key, ref match_dict, ref nodeGraph._nodeDict, out OrderedDictionary<int, int> translation))
			{
				foreach (var item in translation)
				{
					Debug.Log($"{item.Key} : {item.Value}");
				}
				return true;
			}
		}

		return false;
	}

	private static bool MatchNodeGraphInsert(
		int patternGraphIndex,
		int graphIndex,
		ref Dictionary<int, Node> patternDict,
		ref Dictionary<int, Node> graph,
		out OrderedDictionary<int, int> indexTranslation)
	{
		indexTranslation = new OrderedDictionary<int, int>();
		if (patternDict[patternGraphIndex].Node_text != graph[graphIndex].Node_text)
		{
			return false;
		}


		var translationTable = new OrderedDictionary<int, int>
		{
			{ patternGraphIndex, graphIndex }
		};

		return CheckNodeValidity(ref translationTable, ref patternDict, ref graph, ref indexTranslation);
	}

	private static bool CheckNodeValidity(
		ref OrderedDictionary<int, int> translationTable,
		ref Dictionary<int, Node> patternDict,
		ref Dictionary<int, Node> graph,
		ref OrderedDictionary<int, int> indextranslation
		)
	{
		var lastTableEntry = translationTable.Last();
		var patternNode = patternDict[lastTableEntry.Key];
		var graphNode = graph[lastTableEntry.Value];

		if (patternNode.Node_text != graphNode.Node_text)
		{
			return false;
		}

		foreach (var patternConnection in patternNode.ConnectedNodes)
		{
			// if this node is cyclic
			if (translationTable.TryGetValue(patternConnection, out int preexistingGraphConneciton))
			{
				if (!graphNode.ConnectedNodes.Contains(preexistingGraphConneciton))
				{
					return false;
				}
				else
				{
					continue;
				}
			}

			foreach (var graphConnection in graphNode.ConnectedNodes)
			{
				var cloned = new OrderedDictionary<int, int>(translationTable);
				cloned.Add(patternConnection, graphConnection);
				if (CheckNodeValidity(ref cloned, ref patternDict, ref graph, ref indextranslation))
				{
					goto possibleMatch;
				}
			}
			return false;

		possibleMatch:;
		}
		// reverse lookup 
		List<int> taggedIndices = new List<int>();

		foreach (var pattern in patternDict)
		{
			if (!pattern.Value.ConnectedNodes.Contains(lastTableEntry.Key))
			{
				continue;
			}
			if (translationTable.ContainsKey(pattern.Key))
			{
				continue;
			}
			foreach (var unconnectednode in graph.Where(i => i.Value.ConnectedNodes.Contains(lastTableEntry.Value)))
			{
				if ( taggedIndices.Contains(unconnectednode.Key))
				{
					continue;
				}
				var cloned = new OrderedDictionary<int, int>(translationTable);
				cloned.Add(pattern.Key, unconnectednode.Key);
				if (CheckNodeValidity(ref cloned, ref patternDict, ref graph, ref indextranslation))
				{
					taggedIndices.Add(unconnectednode.Key);
					goto possibleMatch;
				}
			}
			return false;

		possibleMatch:;

		}

		foreach (var clone in translationTable)
		{
			if (!indextranslation.ContainsKey(clone.Key))
			{
				indextranslation.Add(clone.Key, clone.Value);
			}
		}
		return true;
	}
}

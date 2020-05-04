using mattmc3.Common.Collections.Generic;
using System.Collections.Generic;
using System.IO;
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
				if (StringUtils.ContainsGeneralization(start_string, grammar.LeftHand))
				{
					start_string = StringUtils.GeneralizedReplaceFirst(start_string, grammar.LeftHand, grammar.RightHand);
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

	/// <summary>
	/// applies the applicable node grammars from <paramref name="nodeGrammars"/> in the sequence described by the string <paramref name="ruleSequence"/> onto the graph <paramref name="nodeGraph"/> example sequence : "ABC" will transform the nodegraph according to the rules labeled "A" then "B" then "C" if they exist withing the supplied grammar rules
	/// </summary>
	/// <param name="ruleSequence">sequence of rules to apply</param>
	/// <param name="nodeGrammars">all possible rules to apply</param>
	/// <param name="nodeGraph">the graph to be cloned and transformed</param>
	/// <returns>the transformed node graph</returns>
	public static NodeGraph ApplyNodeGrammars(string ruleSequence, ref List<NodeGrammar> nodeGrammars, NodeGraph nodeGraph)
	{
		if (ruleSequence == null)
		{
			return nodeGraph;
		}

		NodeGraph output = (NodeGraph)nodeGraph.Clone();

		// TODO : make it possible for rules to have a label of more than one character
		for (int ruleIndex = 0; ruleIndex < ruleSequence.Length; ruleIndex++)
		{
			string ruleLabel = ruleSequence[ruleIndex].ToString();
			// collect all the rules with the right label
			var rules = nodeGrammars.Where((NodeGrammar gram) => gram.Name == ruleLabel);
			if (rules.Count() == 0)
			{
				// no rules match this label
				continue;
			}
			// try all the applicable rules
			foreach (var rule in rules)
			{
				if (IsGrammarApplicable(rule, ref output, out OrderedDictionary<int, int> translationTable))
				{
					// if the grammar is applicable we get a valid translation table back, this table will give  us translations for which IDs in the graph match which IDs in the left hand side of the grammar rule
					// we found one that works, apply it and go to the next step in the sequence
					ApplyNodeRule(rule, ref output, ref translationTable);
					break;
				}
			}
		}
		return output;
	}

	public static List<StringGrammarRule> ImportGrammars(string fileDir)
	{
		// TODO throw exception
		try
		{
			StreamReader reader = new StreamReader(fileDir);
			var jsonString = reader.ReadToEnd();
			var Output = JsonUtility.FromJson<SerializableGrammars>(jsonString).Values;
			reader.Close();
			reader.Dispose();
			return Output;
		}
		catch (FileNotFoundException ex)
		{
			//Debug.LogError(ex.ToString());
			return null;
		}
	}

	/// <summary>
	/// determines which nodes exist on the right hand side of the grammar but not on the left hand and ads them to the translation table
	/// </summary>
	/// <param name="rule"></param>
	/// <param name="nodeGraph"></param>
	/// <param name="translationTable"></param>
	private static void AddMissingNodes(NodeGrammar rule, ref NodeGraph nodeGraph, ref OrderedDictionary<int, int> translationTable)
	{
		// we use the first node as a reference point to offset the positions
		var rightHandPosition = rule.RightHand.NodeDict.First().Value.Pos;
		var graphPosition = nodeGraph.NodeDict[translationTable.First().Value].Pos;
		// add all new nodes created by the rule and give them the proper index
		foreach (var rhNode in rule.RightHand.NodeDict)
		{
			// if the node exists on the right hand but no the left hand it's a missing node and we add it
			if (!rule.LeftHand.NodeDict.ContainsKey(rhNode.Key))
			{
				var newNode = new Node()
				{
					Node_text = rhNode.Value.Node_text,
					Value = rhNode.Value.Value,
					Pos = graphPosition + rhNode.Value.Pos - rightHandPosition
				};

				var newIndex = nodeGraph.AddNode(newNode);
				translationTable.Add(rhNode.Key, newIndex);
			}
		}
	}

	/// <summary>
	/// applies a single node graph translation where a matching precept from the left hand side, found in the graph <paramref name="nodeGraph"/> is applied according to the <paramref name="nodeGraph"/> e.g.  the tranlsation table has an entry [1,5] thus the node with ID 5 in <paramref name="nodeGraph"/> matches ID 1 in <paramref name="rule"/>.LeftHand and will thus be replace by the node linked to by ID 5 of <paramref name="rule"/>.RightHand
	/// </summary>
	/// <param name="rule"></param>
	/// <param name="nodeGraph"></param>
	/// <param name="translationTable"></param>
	private static void ApplyNodeRule(NodeGrammar rule, ref NodeGraph nodeGraph, ref OrderedDictionary<int, int> translationTable)
	{
		// nodes existing in the replacement but not in the translation table are added
		AddMissingNodes(rule, ref nodeGraph, ref translationTable);

		foreach (var translation in translationTable)
		{
			// the node that is to be transformed
			var node = nodeGraph.NodeDict[translation.Value];
			// the Key indicates the LeftHand node that matched with this one, we thus want the matching righthand to replace it
			if (rule.RightHand.NodeDict.TryGetValue(translation.Key, out Node replacementNode))
			{
				// * nodes won't have their values changed, only their connections are modified
				if (replacementNode.Node_text != "*")
				{
					node.Node_text = replacementNode.Node_text;
					node.Value = replacementNode.Value;
				}
				List<int> newConnections = new List<int>();
				// maintain the connections this node had and translate them to the new IDs
				foreach (var connection in node.ConnectedNodes)
				{
					if (!translationTable.Values.Contains(connection))
					{
						newConnections.Add(connection);
					}
				}
				// add new connections as described in the grammar
				foreach (var connection in replacementNode.ConnectedNodes)
				{
					if (translationTable.TryGetValue(connection, out int index))
					{
						newConnections.Add(index);
					}
				}
				node.ConnectedNodes = newConnections;
			}
			else
			{
				// this node doesn't exist in the right hand thus is indicates a deletion
				nodeGraph.Delete(node);
			}
		}
	}

	/// <summary>
	/// returns true if the grammar rule <paramref name="rule"/> has a left hand side that is a subgraph of <paramref name="nodeGraph"/>,
	/// </summary>
	/// <param name="rule">the rule containing the subgraph to match against</param>
	/// <param name="nodeGraph">the supergraph</param>
	/// <param name="translationTable">the IDs of the subgraph as they corrolate to IDs on the supergraph</param>
	/// <returns></returns>
	private static bool IsGrammarApplicable(NodeGrammar rule, ref NodeGraph nodeGraph, out OrderedDictionary<int, int> translationTable)
	{
		translationTable = null;
		var subgraphDict = rule.LeftHand.NodeDict;
		var firstSubgraphID = subgraphDict.Keys.Min();
		var firstSubgraphNode = subgraphDict[firstSubgraphID].Node_text;
		// we brute force check all the nodes in the super graph to see if we can start inserting the subgraph there
		//OrderedDictionary<int, int> translation = new OrderedDictionary<int, int>();
		foreach (var superGraphNode in nodeGraph.NodeDict)
		{
			if (IsPlacementValid(new OrderedDictionary<int, int>(), firstSubgraphID, superGraphNode.Key, ref subgraphDict, nodeGraph.NodeDict, out OrderedDictionary<int, int> translation))
			{
				translationTable = translation;
				return true;
			}
		}
		return false;
	}

	private static bool IsIsomorphicSubGraph(
		ref OrderedDictionary<int, int> internalTranslationTable,
		ref Dictionary<int, Node> subgraph,
		ref Dictionary<int, Node> supergraph,
		ref OrderedDictionary<int, int> indextranslation
		)
	{
		var lastTranslation = internalTranslationTable.Last();
		var subgraphNode = subgraph[lastTranslation.Key];
		var superGraphNode = supergraph[lastTranslation.Value];

		//if (subgraphNode.Node_text != superGraphNode.Node_text)
		if (!StringUtils.CompareGeneralizedString(subgraphNode.Node_text, superGraphNode.Node_text) || !StringUtils.MatchComparators(subgraphNode.Node_text, subgraphNode.ConnectedNodes.Count))
		{
			// these nodes are dissimilars so this isn't a valid subgraph
			return false;
		}

		foreach (var subgraphNodeConnectedID in subgraphNode.ConnectedNodes)
		{
			// if this node is cyclic
			if (internalTranslationTable.TryGetValue(subgraphNodeConnectedID, out int preexistingConnecting))
			{
				// if one of the connected nodes is on the translation table, the translation must match the connections of the node in the supergraph, if we don't do this our subgraph could have connections that don't match the ones described in the left hand grammar
				if (superGraphNode.ConnectedNodes.Contains(preexistingConnecting))
				{
					// this connection is expected to exist
					continue;
				}
				else
				{
					// this one is extra thus the match is invalid
					return false;
				}
			}
			// the connection is not in the translation table so we check the supergraph
			foreach (var supergraphNodeConnectedID in superGraphNode.ConnectedNodes)
			{
				// we assume this connection is valid and add it to the dictionary
				var cloned = new OrderedDictionary<int, int>(internalTranslationTable)
				{
					{ subgraphNodeConnectedID, supergraphNodeConnectedID }
				};
				// check recursively
				if (IsIsomorphicSubGraph(ref cloned, ref subgraph, ref supergraph, ref indextranslation))
				{
					foreach (var clone in cloned)
					{
						if (!internalTranslationTable.ContainsKey(clone.Key))
						{
							internalTranslationTable.Add(clone.Key, clone.Value);
						}
					}
					goto possibleMatch;
				}
			}
			// the necessarily connection is not found so this match is invalid
			return false;

		possibleMatch:;
		}

		// at this point we've confirmed the forward lookup but we need to make sure the incoming connections match up as well
		// reverse lookup

		List<int> taggedIndices = new List<int>();

		// brute force trough all the nodes

		foreach (var potentionSubgraphConnectedNode in subgraph)
		{
			// filter out all the nodes that aren't connected
			if (!potentionSubgraphConnectedNode.Value.ConnectedNodes.Contains(lastTranslation.Key))
			{
				continue;
			}
			// filter out nodes we've already added to the translation table
			if (internalTranslationTable.ContainsKey(potentionSubgraphConnectedNode.Key))
			{
				continue;
			}
			// parse trough the connections that exist in the supergraph

			var unconnectedSuperGraphNodes = from keyval in supergraph
											 where keyval.Value.ConnectedNodes.Contains(lastTranslation.Value)
											 && !taggedIndices.Contains(keyval.Key)
											 select keyval;

			foreach (var unconnectedSuperGraphNode in unconnectedSuperGraphNodes)
			{
				// avoid double checking across multiple nodes
				if (internalTranslationTable.ContainsValue(unconnectedSuperGraphNode.Key))
				{
					continue;
				}
				// assume a reverse connection exists
				var cloned = new OrderedDictionary<int, int>(internalTranslationTable)
				{
					{ potentionSubgraphConnectedNode.Key, unconnectedSuperGraphNode.Key }
				};
				// check recursively
				if (IsIsomorphicSubGraph(ref cloned, ref subgraph, ref supergraph, ref indextranslation))
				{
					taggedIndices.Add(unconnectedSuperGraphNode.Key);

					foreach (var clone in cloned)
					{
						if (!internalTranslationTable.ContainsKey(clone.Key))
						{
							internalTranslationTable.Add(clone.Key, clone.Value);
						}
					}
					goto possibleMatch;
				}
			}
			// we've found a node that is supposed to be connected in the subgraph but isn't connected in the supergraph thus the match failed
			return false;

		possibleMatch:;
		}
		indextranslation = new OrderedDictionary<int, int>(internalTranslationTable);
		return true;
	}

	private static bool IsPlacementValid(OrderedDictionary<int, int> input_translation, int subgraphID, int superGraphID, ref Dictionary<int, Node> subgraphDict, Dictionary<int, Node> superGraph, out OrderedDictionary<int, int> translation)
	{
		//IsSubGraphIsomorphicAtPosition(firstSubgraphID, superGraphNode.Key, ref subgraphDict, nodeGraph.NodeDict, ref translation)
		translation = null;
		var tempTranslation = new OrderedDictionary<int, int>(input_translation);
		if (IsSubGraphIsomorphicAtPosition(subgraphID, superGraphID, ref subgraphDict, superGraph, ref tempTranslation))
		{
			var missingNode = subgraphDict.FirstOrDefault((kv) => !tempTranslation.ContainsKey(kv.Key));
			if (missingNode.Value == null)
			{
				translation = tempTranslation;
				return true;
			}
			var collection = from kv in superGraph
							 where !tempTranslation.Values.Contains(kv.Key)
							 select kv;
			foreach (var super in collection)
			{
				if (IsPlacementValid(tempTranslation, missingNode.Key, super.Key, ref subgraphDict, superGraph, out OrderedDictionary<int, int> finalTranslation))
				{
					translation = finalTranslation;
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// returns true if the <paramref name="superGraph"/> is isomorphic to the <paramref name="superGraph"/> with the assumption that the node in the subgraph with ID <paramref name="subgraphID"/> matches the node in the supergraph with ID <paramref name="superGraphID"/>
	/// outputs the translation of the IDs from the subgraph to the supergraph
	/// </summary>
	/// <param name="subgraphID"></param>
	/// <param name="superGraphID"></param>
	/// <param name="subGraph"></param>
	/// <param name="superGraph"></param>
	/// <param name="indexTranslation"></param>
	/// <returns></returns>
	private static bool IsSubGraphIsomorphicAtPosition(
		int subgraphID,
		int superGraphID,
		ref Dictionary<int, Node> subGraph,
		Dictionary<int, Node> superGraph,
		ref OrderedDictionary<int, int> indexTranslation)
	{
		if (!StringUtils.CompareGeneralizedString(subGraph[subgraphID].Node_text, superGraph[superGraphID].Node_text) || !StringUtils.MatchComparators(subGraph[subgraphID].Node_text, superGraph[superGraphID].ConnectedNodes.Count))
		{
			return false;
		}

		var translationTable = new OrderedDictionary<int, int>(indexTranslation)
		{
			{ subgraphID, superGraphID }
		};

		return IsIsomorphicSubGraph(ref translationTable, ref subGraph, ref superGraph, ref indexTranslation);
	}
}

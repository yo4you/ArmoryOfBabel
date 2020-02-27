using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct NodeGrammar
{
	public string Name;
	public NodeGraph LeftHand;
	public NodeGraph RightHand;

	public static explicit operator NodeGrammar(Serializable_Grammar v)
	{
		return new NodeGrammar
		{
			LeftHand = (NodeGraph)v.LeftHand,
			RightHand = (NodeGraph)v.RightHand,
			Name = v.Name,
		};
	}
}

public static class SerializableNodeGrammars_Converter
{
	internal static string ToJson(List<NodeGrammar> grammars)
	{
		var serializable_grams = new Serializable_Grammars();
		foreach (var grammar in grammars)
		{
			serializable_grams.Values.Add((Serializable_Grammar)grammar);
		}
		return JsonUtility.ToJson(serializable_grams,true);
	}

	internal static List<NodeGrammar> FromJson(string json)
	{
		var outp = new List<NodeGrammar>();

		var data = JsonUtility.FromJson<Serializable_Grammars>(json);
		foreach (var gram in data.Values)
		{
			outp.Add((NodeGrammar)gram);
		}
		return outp;
	}

}
[Serializable]
public class Serializable_Grammars
{
	public List<Serializable_Grammar> Values = new List<Serializable_Grammar>();
}
[Serializable]
public class Serializable_Grammar
{
	public string Name;
	public Serializable_NodeGraph LeftHand;
	public Serializable_NodeGraph RightHand;

	public static explicit operator Serializable_Grammar(NodeGrammar v)
	{
		return new Serializable_Grammar()
		{
			Name = v.Name,
			LeftHand = (Serializable_NodeGraph)v.LeftHand,
			RightHand = (Serializable_NodeGraph)v.RightHand,
		};
	}
}
[Serializable]
public class Serializable_NodeGraph
{
	public List<int> Keys;
	public List<Node> Values;
	public int lastID;

	public static explicit operator Serializable_NodeGraph(NodeGraph v)
	{
		return new Serializable_NodeGraph()
		{
			Keys = v._nodeDict.Keys.ToList(),
			Values = v._nodeDict.Values.ToList(),
			lastID = v._lastId
		};
	}
}
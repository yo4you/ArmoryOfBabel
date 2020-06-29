using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
/// <summary>
/// used to analyze and balance mechanics in way not yet possible with our grammar system
/// this involves adjusting attack damage values to reach a specific average
/// this also involves adjusting resources (UI elementS) so that non-functional systems are pruned and other systems are sped up/slowed down
/// </summary>
internal static class MechanicBalancer
{
	// resources with a standard deviation below this value are considered non-functional
	private const float _lowDeviationTreshold = 0.15f;
	// the maximum amount damage of an attack can be multiplied/divided by
	private const float _maxBalancingFactor = 4f;
	private static Dictionary<Node, List<float>> _attackObservation = null;
	private static bool _standby = true;
	private static Dictionary<Node, UIObservation> _uiObservations = null;
	/// <summary>
	/// starts recording incoming observations
	/// </summary>
	public static void StartAnalyze()
	{
		if (!_standby)
		{
			Debug.LogError("trying to start analysis but one is already running");
			return;
		}
		_standby = false;
		_attackObservation = new Dictionary<Node, List<float>>();
		_uiObservations = new Dictionary<Node, UIObservation>();
	}

	/// <summary>
	/// stop recording observations
	/// applies the balancing formula to the node graph
	/// </summary>
	/// <param name="nodeGraph"></param>
	/// <param name="desiredAverage">the desired average damage of all attacks</param>
	public static void EndAnalyze(ref NodeGraph nodeGraph, float desiredAverage)
	{
		_standby = true;


		// analyse the resources 
		//Debug.Log("ui data : \n ============");
		foreach (var observationKV in _uiObservations)
		{
			
			Node node = observationKV.Key;
			float cap = GetCapacity(nodeGraph, node);
			if (IsHeldNode(node, nodeGraph)) 
			{ 
				continue; 
			}
			var observation = observationKV.Value;
			// used to calculate the std
			var average = observation.ValueHistory.Average();
			var sumOfSquaresOfDifferences = observation.ValueHistory.Select(val => (val - average) * (val - average)).Sum();
			// normalized by dividing these values by the capacity of the ui node
			var standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / observation.ValueHistory.Count) / cap;
			var spread = new Vector2(observation.ValueHistory.Min(), observation.ValueHistory.Max()) / cap;

			//Debug.Log($"Diff: {observation.Diff.magnitude / cap} || STD: {standardDeviation * _uiObservations.Count}   || Spread : {spread}");
			if (standardDeviation < .001f) 
			{
				// these mechanics have more than one thing wrong with them and are thus unsalvagable
				DeleteElement(node, ref nodeGraph);
			}
			else if (observation.Diff.magnitude < cap || standardDeviation * _uiObservations.Count < _lowDeviationTreshold)
			{
				// these mechanics usually can be fixed by adjusting the ingoing resources
				RebalanceDisfunctionalResource(node, ref nodeGraph);
			}
			else if (observation.Diff.magnitude < cap * 5)
			{
				// these mechanics don't move enough, this can usually be because their threshold is set too high 
				foreach (var thresholdID in node.ConnectedNodes)
				{
					nodeGraph.NodeDict[thresholdID].Value *= .5f;
				}
			}
		}

		nodeGraph = BalanceDamageValues(nodeGraph, desiredAverage);
	}

	private static bool IsHeldNode(Node node, NodeGraph nodeGraph)
	{
		return nodeGraph.GetAffectors(node, i => i.Node_text == "sign_held").Count() != 0;
	}

	/// <summary>
	/// checks if a UI node can be rebalanced by flipping the incoming resources, deletes it otherwise 
	/// </summary>
	/// <param name="node"></param>
	/// <param name="nodeGraph"></param>
	private static void RebalanceDisfunctionalResource(Node node, ref NodeGraph nodeGraph)
	{
		var ingoingAffectors = nodeGraph.GetAffectors(node, i => i.Node_text == "VAL");
		if (ingoingAffectors.Count() > 1 && 
			(ingoingAffectors.All(i => i.Value <= 0f) ||
				ingoingAffectors.All(i => i.Value > 0f)))
		{
			List<Node> affectors = ingoingAffectors.ToList();
			// flip every other effector value 
			for (int i = 0; i < affectors.Count; i += 2)
			{
				nodeGraph.NodeDict[nodeGraph.GetIdFromNode(affectors[i])].Value *= -1f;
			}
		}
		else
		{
			DeleteElement(node, ref nodeGraph);
		}
	}

	/// <summary>
	/// deleted UI node and associated connections
	/// </summary>
	/// <param name="node"></param>
	/// <param name="nodeGraph"></param>
	private static void DeleteElement(Node node, ref NodeGraph nodeGraph)
	{

		// TODO fix special moves still showing up when the attached resource is deleted
		for (int i = node.ConnectedNodes.Count - 1; i >= 0; i--)
		{
			int connectedID = node.ConnectedNodes[i];
			nodeGraph.Delete(connectedID);
		}
		nodeGraph.Delete(nodeGraph.GetIdFromNode(node));
	}

	/// <summary>
	/// returns the capacity of the UI node in the node graph
	/// </summary>
	/// <param name="nodeGraph"></param>
	/// <param name="uiNode"></param>
	/// <returns></returns>
	private static float GetCapacity(NodeGraph nodeGraph, Node uiNode)
	{
		var cap = nodeGraph.GetAffectors(uiNode, (i) => i.Node_text == "UIC");
		return cap.FirstOrDefault().Value;
	}
	/// <summary>
	/// attempts to balance to shift the outgoing damage of moves so that they match the average damage
	/// </summary>
	/// <param name="nodeGraph"></param>
	/// <param name="averageDamage"></param>
	/// <returns></returns>
	private static NodeGraph BalanceDamageValues(NodeGraph nodeGraph, float averageDamage)
	{
		// we might have more than 2 attacks but we'll focus on the two most frequent (any subsequent move will be treated as the second most frequent move)
		var attacks = _attackObservation.OrderByDescending(i =>
		{
			// cull special attacks with 0 damage even if they're more frequent
			return i.Value.Count - (i.Value.FirstOrDefault() > .1f ? 0 : 999_999_999);
		}).ToList();

		// we have one attack, bring it up to average
		if (attacks.Count(i => i.Value.FirstOrDefault() > .1f) == 1)
		{
			var mult = averageDamage / attacks.First().Value.Average();
			AdjustOutput(ref nodeGraph, attacks.First().Key, mult);
		}
		else
		{
			var a1 = attacks[0];
			var a2 = attacks[1];

			/// the formula for balancing:
			/// let a1 and a2 be the first and second attack,
			/// let p1 and p2 be the amount of times those attacks are performed,
			/// let C and K be the average damage of a1 and a2,
			/// let A be the desired average damage
			/// x and y shall be the multipliers for the damage of a1 and a2
			/// Both x and x are greater than 1
			/// y = -(cp1x - (p1 + p2)A) / kp2
			/// range for x is { 1, (-kp2 + (p1+p2)A)) / cp1 }
			/// T is the total desired damage dealt : (p1+p2) A

			var p1 = a1.Value.Count;
			var p2 = a2.Value.Count;
			var c = a1.Value.Average();
			var k = a2.Value.Average();
			var a = averageDamage;
			var T = (p1 + p2) * a;

			// we want the "right" side of the graph so we interpolate between 0.7-1.0 this ensures our primary attack remains weaker than our less frequent secondary attack
			float x = Mathf.Lerp(1f, (-k * p2 + T) / (c * p1), .7f + UnityEngine.Random.Range(0f, .3f));
			float y = -(c * p1 * x - T) / (k * p2);
// 
// 			Debug.Log($"balance {x} / {y}");
// 			Debug.Log("attack data : \n ============");
// 			foreach (var attack in _attackObservation.Values)
// 			{
// 				Debug.Log($"{ attack.Average()} : {attack.Count}");
// 			}
			AdjustOutput(ref nodeGraph, a1.Key, x);
			foreach (var attack in attacks)
			{
				if (attack.Key != a1.Key)
				{
					AdjustOutput(ref nodeGraph, attack.Key, y);
				}
			}
		}

		return nodeGraph;
	}

	/// <summary>
	/// scales the outgoing damage associated with the out node 
	/// </summary>
	/// <param name="nodeGraph"></param>
	/// <param name="node"></param>
	/// <param name="mult">amount it should be scaled by</param>
	private static void AdjustOutput(ref NodeGraph nodeGraph, Node node, float mult)
	{
		mult = Mathf.Clamp(mult, 1/ _maxBalancingFactor, _maxBalancingFactor);
		var id = nodeGraph.GetIdFromNode(node);
		foreach (var nodeEntry in from nodeEntry in nodeGraph._nodeDict
								  where nodeEntry.Value.Node_text == "DMG"
								  where nodeEntry.Value.ConnectedNodes.Contains(id)
								  select nodeEntry)
		{
			nodeGraph.NodeDict[nodeEntry.Key].Value *= mult;
		}
	}
	/// <summary>
	/// records an observation of an outgoing attack 
	/// </summary>
	/// <param name="node"></param>
	/// <param name="damage"></param>
	public static void RegisterAttackObservation(Node node, float damage)
	{
		if (_standby)
		{
			Debug.LogError("trying to register observation but the analysis has ended");
			return;
		}
		if (_attackObservation.TryGetValue(node, out List<float> list))
		{
			list.Add(damage);
		}
		else
		{
			_attackObservation[node] = new List<float>() { damage };
		}
	}
	/// <summary>
	/// records an observation of the state of the ui elements
	/// </summary>
	/// <param name="node"></param>
	public static void RegisterUIObservation(Node node)
	{
		if (_standby)
		{
			return;
		}
		if (!_uiObservations.ContainsKey(node))
		{
			_uiObservations[node] = new UIObservation();
			RegisterUIObservation(node);
		}
		else
		{
			var observation = _uiObservations[node];
			var diff = node.Value - observation.LastVal;
			observation.LastVal = node.Value;

			var newdiff = observation.Diff;

			// diff shows the amount removed and added as parts of a vector2
			newdiff[(Math.Sign(diff) + 1) / 2] += diff;
			observation.Diff = newdiff;
			_uiObservations[node] = observation;
		}
	}
}
/// <summary>
/// container for storing the state of analyzed resources 
/// </summary>
internal class UIObservation
{
	public Vector2 Diff { get; set; } = new Vector2();
	public float LastVal { get => ValueHistory.LastOrDefault(); set => ValueHistory.Add(value); }
	public List<float> ValueHistory { get; set; } = new List<float>();
}
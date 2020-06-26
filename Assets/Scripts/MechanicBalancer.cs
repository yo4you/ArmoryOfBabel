﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class MechanicBalancer
{
	private static Dictionary<Node, List<float>> _attackObservation = null;
	private static bool _ended = true;
	private static Dictionary<Node, UIObservation> _uiObservations = null;

	private enum ResourceDiffDiagnosis
	{
		BALANCED,
		HIGH_AMPLITUDE,
		LOW_AMPLITUDE,
		SURPLUS,
		SCARCITY,
		DERANGED
	}

	// 	private static Dictionary<Vector2, ResourceDiffDiagnosis> _examples = new Dictionary<Vector2, ResourceDiffDiagnosis>
	// 		{
	// 			{new Vector2(-1,0).normalized, ResourceDiffDiagnosis.DERANGED },
	// 			{new Vector2(-1,0.5f).normalized, ResourceDiffDiagnosis.SCARCITY },
	// 			{new Vector2(-1,1f).normalized, ResourceDiffDiagnosis.BALANCED },
	// 			{new Vector2(-0.5f,1f).normalized, ResourceDiffDiagnosis.SURPLUS },
	// 			{new Vector2(-0,1f).normalized, ResourceDiffDiagnosis.DERANGED },
	// 		};

	public static void StartAnalyze()
	{
		if (!_ended)
		{
			Debug.LogError("trying to start analysis but one is already running");
			return;
		}
		_ended = false;
		_attackObservation = new Dictionary<Node, List<float>>();
		_uiObservations = new Dictionary<Node, UIObservation>();
	}

	public static void EndAnalyze(ref NodeGraph nodeGraph, float averageDamage)
	{
		_ended = true;

		Debug.Log("ui data : \n ============");
		foreach (var observationKV in _uiObservations)
		{
			float cap = 0;
			cap = GetCapacity(nodeGraph, observationKV.Key);

			var observation = observationKV.Value;
			ResourceDiffDiagnosis diagnosis = DiagnoseNodeDiff(observation.Diff);
			var average = observation.ValueHistory.Average();
			var sumOfSquaresOfDifferences = observation.ValueHistory.Select(val => (val - average) * (val - average)).Sum();
			var standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / observation.ValueHistory.Count) / cap;
			var spread = new Vector2(observation.ValueHistory.Min(), observation.ValueHistory.Max()) / cap;

			Debug.Log($"Diff: {observation.Diff / cap} || STD: {standardDeviation}   || State : {diagnosis} || Spread : {spread}");
		}

		nodeGraph = BalanceDamageValues(nodeGraph, averageDamage);
	}

	private static float GetCapacity(NodeGraph nodeGraph, Node node)
	{
		var id = nodeGraph.GetIdFromNode(node);
		foreach (var item in nodeGraph.NodeDict)
		{
			if (item.Value.Node_text == "UIC" && item.Value.ConnectedNodes.Contains(id))
			{
				return item.Value.Value;
			}
		}
		return 10f;
	}

	private static ResourceDiffDiagnosis DiagnoseNodeDiff(Vector2 diff)
	{
		var diagnosis = ResourceDiffDiagnosis.BALANCED;
		//diagnosis = _examples.OrderByDescending(i => Mathf.Abs(Vector2.Dot(i.Key, diff.normalized))).First().Value;

		if (diagnosis == ResourceDiffDiagnosis.BALANCED)
		{
			var magnitude = diff.magnitude;
			diagnosis = ResourceDiffDiagnosis.LOW_AMPLITUDE;
			if (magnitude > 200)
				diagnosis = ResourceDiffDiagnosis.BALANCED;
			if (magnitude > 2000)
				diagnosis = ResourceDiffDiagnosis.HIGH_AMPLITUDE;
		}

		return diagnosis;
	}

	private static NodeGraph BalanceDamageValues(NodeGraph nodeGraph, float averageDamage)
	{
		var attacks = _attackObservation.OrderByDescending(i =>
		{
			return i.Value.Count - (i.Value.FirstOrDefault() > .1f ? 0 : 999_999_999);
		}).ToList();

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

			float x = Mathf.Lerp(1f, (-k * p2 + T) / (c * p1), .7f + UnityEngine.Random.Range(0f, .3f));
			float y = -(c * p1 * x - T) / (k * p2);

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

	private static void AdjustOutput(ref NodeGraph nodeGraph, Node node, float mult)
	{
		mult = Mathf.Clamp(mult, 0.25f, 4f);
		var id = nodeGraph.GetIdFromNode(node);
		foreach (var nodeEntry in from nodeEntry in nodeGraph._nodeDict
								  where nodeEntry.Value.Node_text == "DMG"
								  where nodeEntry.Value.ConnectedNodes.Contains(id)
								  select nodeEntry)
		{
			nodeGraph.NodeDict[nodeEntry.Key].Value *= mult;
		}
	}

	public static void RegisterAttackObservation(Node node, float damage)
	{
		if (_ended)
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

	public static void RegisterUIObservation(Node node)
	{
		if (_ended)
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
			newdiff[(Math.Sign(diff) + 1) / 2] += diff;
			observation.Diff = newdiff;
			_uiObservations[node] = observation;
		}
	}
}

internal class UIObservation
{
	public Vector2 Diff { get; set; } = new Vector2();
	public float LastVal { get => ValueHistory.LastOrDefault(); set => ValueHistory.Add(value); }
	public List<float> ValueHistory { get; set; } = new List<float>();
}
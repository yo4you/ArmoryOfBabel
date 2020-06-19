using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

internal static class MechanicBalancer
{
	private static Dictionary<Node, List<float>> _attackObservation = null;
	private static Dictionary<Node, Vector2> _uiDiff = null;
	private static bool _ended = true;

	public static void StartAnalyze()
	{
		if (!_ended)
		{
			Debug.LogError("trying to start analysis but one is already running");
			return;
		}
		_ended = false;
		_attackObservation = new Dictionary<Node, List<float>>();
		_uiDiff = new Dictionary<Node, Vector2>();
	}

	public static void EndAnalyze(ref NodeGraph nodeGraph, float averageDamage)
	{
		_ended = true;

		Debug.Log("attack data : \n ============");
		foreach (var attack in _attackObservation.Values)
		{
			Debug.Log($"{ attack.Average()} : {attack.Count}");
		}
		Debug.Log("ui data : \n ============");
		foreach (var diff in _uiDiff.Values)
		{
			Debug.Log(diff);
		}

		var attacks = _attackObservation.OrderByDescending(i => i.Value.Count).ToList();

		if (attacks.Count == 1)
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

			float x = Mathf.Lerp(1f, (-k * p2 + T) / (c * p1), .5f + UnityEngine.Random.Range(0f, .5f));
			float y = -(c * p1 * x - T) / (k * p2);

			Debug.Log($"balance {x} / {y}");

			AdjustOutput(ref nodeGraph, a1.Key, x);
			foreach (var attack in attacks)
			{
				if (attack.Key != a1.Key)
					AdjustOutput(ref nodeGraph, attack.Key, y);
			}
		}
	}

	private static void AdjustOutput(ref NodeGraph nodeGraph, Node node, float mult)
	{
		var id = nodeGraph.GetIdFromNode(node);
		foreach (var nodeEntry in from nodeEntry in nodeGraph._nodeDict
								  where nodeEntry.Value.Node_text == "DMG"
								  where nodeEntry.Value.ConnectedNodes.Contains(id)
								  select nodeEntry)
		{
			nodeEntry.Value.Value *= mult;
			Debug.Log("ID :" + nodeGraph.GetIdFromNode(nodeEntry.Value));
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

	public static void RegisterUIObservation(Node node, float diff)
	{
		if (_ended)
		{
			return;
		}
		if (_uiDiff.ContainsKey(node))
		{
			var newdiff = _uiDiff[node];
			newdiff[(Math.Sign(diff) + 1) / 2] += diff;
			_uiDiff[node] = newdiff;
		}
		else
		{
			_uiDiff[node] = new Vector2();
			RegisterUIObservation(node, diff);
		}
	}
}
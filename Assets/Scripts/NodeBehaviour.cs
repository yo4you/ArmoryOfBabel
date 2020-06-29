using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// class containing static data nodes need access to as well as the logic to which nodes functions 
/// </summary>
public static class NodeBehaviour
{
	// the types of nodes used for values, these are not intended to activate other nodes in most cases
	public static string[] ValueHoldingNodes = { "VAL", "DMG", "SPD", "TYPE", "COPY", "DT", "SUM", "STAT", "MULT" };
	// callback stack, when an out node is called a new callback will be added. if this is not linked to a callback such as "HIT" the callback will be removed
	public static Stack<NodeActivationCallBack> Callbacks { get; set; } = new Stack<NodeActivationCallBack>();
	public static IPlayerAttackControl PlayerAttacks { get; internal set; }

	public static IPlayerMovement PlayerMovement { get; internal set; }
	/// <summary>
	/// Logical operator, activates when all connected nodes are activated.
	/// </summary>
	public static void SetState_AndNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		node.Active = true;
		int id = graph.GetIdFromNode(node);
		foreach (var potentialConnection in graph.NodeDict)
		{
			if (potentialConnection.Value.ConnectedNodes.Contains(id))
			{
				if (!potentialConnection.Value.Active)
				{
					node.Active = false;
					return;
				}
			}
		}
	}
	/// <summary>
	/// base state of nodes that don't have special behaviour, will let a signal pass trough
	/// </summary>
	public static void SetState_GenericNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		node.Active = state;
	}
	/// <summary>
	/// Callback node, will push a callback to the stack. The callback contains a reference to the last activated Out node. An external system (for example collision response) can then cause this Hit node to be activated.
	/// </summary>
	public static void SetState_HitNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (prevNode == null)
		{
			node.Active = state;
		}
		else if (state)
		{
			if (Callbacks.Peek().Activatee == null)
			{
				Callbacks.Peek().Activatee = node;
			}
		}
	}
	/// <summary>
	/// Logical operator, activated when no connected node activates it. Does not pass activation signal trough.	
	/// </summary>

	public static void SetState_NotNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (prevNode == null)
		{
			node.Active = state;
		}
		else
		{
			node.Active = !state;
		}
	}
	/// <summary>
	/// Logical operator, activates when the first connected node is activated.
	/// </summary>
	public static void SetState_OrNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		node.Active = false;
		foreach (var connection in node.ConnectedNodes)
		{
			if (graph.NodeDict[connection].Active)
			{
				node.Active = true;
				return;
			}
		}
	}
	/// <summary>
	/// Outputs an attack, controlled by an attack system. Intended to be activated when the attack went through. Can not be activated by Value-like nodes.
	/// </summary>
	public static void SetState_OutNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (node.Active)
		{
			return;
		}

		if (IsValueHoldingNode(prevNode))
		{
			return;
		}
		node.Active = state;

		int id = graph.GetIdFromNode(node);
		var spd = .001f;
		var dmg = .001f;
		var type = 0f;
		var status = -1f;
		foreach (var potentialAffector in graph.NodeDict)
		{
			if (!potentialAffector.Value.ConnectedNodes.Contains(id))
			{
				continue;
			}
			switch (potentialAffector.Value.Node_text)
			{
				case "DMG":
					dmg = Mathf.Max(dmg, potentialAffector.Value.Value);
					continue;

				case "SPD":
					spd = Mathf.Max(spd, potentialAffector.Value.Value);
					continue;

				case "TYPE":
					type = potentialAffector.Value.Value;
					continue;
				case "STAT":
					status = potentialAffector.Value.Value;
					break;

				default:
					continue;
			}
		}
		// checks if the attack can go trough, if it can't this node won't activate
		node.Active = PlayerAttacks.ProccessAttackNode(spd, dmg, (int)type, node, (int)status);
		if (Callbacks.Count != 0 && Callbacks.Peek().Activator == null)
		{
			Callbacks.Peek().Activator = node;
		}
	}
	/// <summary>
	/// Resource values intended to have their state restored by another system and displayed to the player. Activated each frame.
	/// </summary>
	public static void SetState_UINode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (prevNode == null)
		{
			node.Active = state;
		}
		else if (state && prevNode.Node_text == "VAL")
		{
			node.Value += prevNode.Value;
		}
	}
	/// <summary>
	/// Resource values intended to have their state restored by another system and displayed to the player. Activated each frame.
	/// </summary>
	public static void SetState_ValNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		CombineChildValues(prevNode, node, ref graph, state, baseval, false);
	}
	/// <summary>
	/// combines the values of the nodes connected to this one according to <paramref name="addition"/>
	/// when true values will be summed, else, values will be multiplied
	/// </summary>
	internal static void CombineChildValues(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval, bool addition)
	{
		node.Active = state;
		int id = graph.GetIdFromNode(node);
		if (IsValueHoldingNode(prevNode))
		{
			node.Value = baseval;
			foreach (var potentialAffector in graph.NodeDict)
			{
				if (!potentialAffector.Value.ConnectedNodes.Contains(id) || !IsValueHoldingNode(potentialAffector.Value) || !potentialAffector.Value.Active)
				{
					continue;
				}
				var nodeVal = potentialAffector.Value.Value;

				if (addition)
				{
					// addition
					node.Value += nodeVal;
				}
				else
				{
					// multiplication
					node.Value *= nodeVal;
				}
			}
		}
	}
	/// <summary>
	/// When activated by a non-UI node, will subsequently copy the value of any UI node that activates it.
	/// </summary>
	internal static void SetState_CopyNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (prevNode.Node_text != "UI")
		{
			node.Active = state;
		}
		else
		{
			node.Value = (state && prevNode.Value != 0f) ? prevNode.Value : float.Epsilon;
		}
	}
	/// <summary>
	/// activated when the player moves. Can modify the player's move speed.
	/// </summary>
	internal static void SetState_MoveNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseState)
	{
		if (prevNode == null)
		{
			node.Active = state;
		}

		int id = graph.GetIdFromNode(node);
		var spd = 1f;
		foreach (var potentialAffector in graph.NodeDict)
		{
			if (!potentialAffector.Value.ConnectedNodes.Contains(id))
			{
				continue;
			}
			if (potentialAffector.Value.Node_text == "SPD")
			{
				spd = potentialAffector.Value.Value;
			}
		}

		PlayerMovement.SpeedMultiplier = spd;
	}
	/// <summary>
	/// When activated sums up the internal value with that of any nodes connected to this one.
	/// </summary>
	internal static void SetState_SumNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		CombineChildValues(prevNode, node, ref graph, state, baseval, true);
	}
	/// <summary>
	/// Can only be activated by UI nodes with a value that exceeds the internal value.
	/// </summary>
	internal static void SetState_TreshNode(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseval)
	{
		if (prevNode == null)
		{
			node.Active = state;
			return;
		}
		if (prevNode.Active)
		{
			node.Active = prevNode.Value >= node.Value;
		}
	}

	private static bool IsValueHoldingNode(Node prevNode)
	{
		return prevNode != null && ValueHoldingNodes.Contains(prevNode.Node_text);
	}
}
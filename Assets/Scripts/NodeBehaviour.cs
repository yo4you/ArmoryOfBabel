using System.Collections.Generic;

public static class NodeBehaviour
{
	public static Stack<NodeActivationCallBack> Callbacks { get; set; } = new Stack<NodeActivationCallBack>();
	public static PlayerAttackControl PlayerAttacks { get; internal set; }

	public static void SetState_AndNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		if (!state)
		{
			node.Active = false;
			return;
		}
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

	public static void SetState_GenericNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		node.Active = state;
	}

	public static void SetState_HitNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
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

	public static void SetState_NotNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
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

	public static void SetState_OrNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
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

	public static void SetState_OutNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		if (!state)
		{
			node.Active = false;
			return;
		}

		// TODO check cooldowns
		if (prevNode.Node_text == "VAL" || prevNode.Node_text == "DMG" || prevNode.Node_text == "SPD" || prevNode.Node_text == "TYPE" || prevNode.Node_text == "COPY")
		{
			return;
		}
		node.Active = state;

		int id = graph.GetIdFromNode(node);
		var spd = 1f;
		var dmg = 1f;
		var type = 0f;
		foreach (var potentialAffector in graph.NodeDict)
		{
			if (!potentialAffector.Value.ConnectedNodes.Contains(id))
			{
				continue;
			}
			switch (potentialAffector.Value.Node_text)
			{
				case "DMG":
					dmg = potentialAffector.Value.Value;
					continue;

				case "SPD":
					spd = potentialAffector.Value.Value;
					continue;

				case "TYPE":
					type = potentialAffector.Value.Value;
					continue;

				default:
					continue;
			}
		}
		node.Active = PlayerAttacks.ProccessAttackNode(spd, dmg, (int)type, node);
		if (Callbacks.Peek().Activator == null)
		{
			Callbacks.Peek().Activator = node;
		}
	}

	public static void SetState_UINode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		node.Active = state;

		if (state && prevNode.Node_text == "VAL")
		{
			node.Value += prevNode.Value;
		}
	}

	public static void SetState_ValNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		node.Active = state;

		if (prevNode.Node_text == "VAL" || prevNode.Node_text == "DT" || prevNode.Node_text == "COPY")
		{
			var nodeVal = prevNode.Value;
			if (nodeVal == 0f)
			{
				nodeVal = float.Epsilon;
			}
			if (state)
			{
				node.Value *= nodeVal;
			}
			else
			{
				node.Value /= nodeVal;
			}
		}
	}

	internal static void SetState_CopyNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
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

	internal static void SetState_TreshNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		if (prevNode.Active)
		{
			node.Active = prevNode.Value >= node.Value;
		}
		else
		{
			node.Active = false;
		}
	}
}

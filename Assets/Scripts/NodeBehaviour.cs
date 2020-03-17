using UnityEngine;

public static class NodeBehaviour
{
	public static void SetState_AndNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		if (!state)
		{
			node.Active = false;
			return;
		}
		node.Active = true;
		int id = graph.GetIdFromNode(node);
		foreach (var potentialConnection in graph._nodeDict)
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
		node.Active = state;
		if (state)
		{
			Debug.Log("created projectile callback");
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
			if (graph._nodeDict[connection].Active)
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
		if (prevNode.Node_text == "VAL" || prevNode.Node_text == "DMG" || prevNode.Node_text == "SPD" || prevNode.Node_text == "TYPE")
		{
			return;
		}
		node.Active = state;

		int id = graph.GetIdFromNode(node);
		var spd = 1f;
		var dmg = 1f;
		var type = 0f;
		foreach (var potentialAffector in graph._nodeDict)
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

		Debug.Log($"pew projectile spd:{spd} dmg:{dmg} type:{type}");
	}

	public static void SetState_UINode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		node.Active = state;

		if (state && prevNode.Node_text == "VAL")
		{
			var nodeCap = 100f;
			var id = graph.GetIdFromNode(node);
			foreach (var item in graph._nodeDict)
			{
				if (item.Value.Node_text == "UIC" && item.Value.ConnectedNodes.Contains(id))
				{
					nodeCap = item.Value.Value;
				}
			}
			node.Value += prevNode.Value;
			Mathf.Clamp(node.Value, 0, nodeCap);
		}
	}

	public static void SetState_ValNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		node.Active = state;

		if (prevNode.Node_text == "VAL")
		{
			if (state)
			{
				node.Value *= prevNode.Value;
			}
			else
			{
				node.Value /= prevNode.Value;
			}
		}
	}

	internal static void SetState_TreshNode(Node prevNode, Node node, ref NodeGraph graph, bool state)
	{
		if (prevNode.Active)
		{
			node.Active = prevNode.Value > node.Value;
		}
		else
		{
			node.Active = false;
		}
	}
}

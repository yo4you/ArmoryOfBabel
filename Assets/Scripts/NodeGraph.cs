using System;
using System.Collections.Generic;
using System.Linq;

//using UnityEditor;
using UnityEngine;

/// <summary>
/// a class used storing nodes and their relations as well as providing all necessarily methods for controlling them from the editor window
/// </summary>
public class NodeGraph : ICloneable
{
	// last ID dispatched to a node
	public int _lastId;

	// dictionary storing all nodes by their IDs
	public Dictionary<int, Node> _nodeDict = new Dictionary<int, Node>();

	// size of the indication at the end of a connection
	private const float _ballSize = 10;

	// the width of the connection lines
	private const float _lineWidth = 2f;

	// margin around the nodes
	private const int _nodeMargin = 6;

	// size of the nodes
	// TODO don't hard code this
	private const int _nodeSize = 100;

	public Dictionary<int, Node> NodeDict { get => _nodeDict; set => _nodeDict = value; }

	public static explicit operator NodeGraph(Serializable_NodeGraph v)
	{
		var dict = new Dictionary<int, Node>();
		for (int i = 0; i < v.Keys.Count; i++)
		{
			dict.Add(v.Keys[i], v.Values[i]);
		}

		return new NodeGraph()
		{
			NodeDict = dict,
			_lastId = v.lastID
		};
	}

	/// <summary>
	/// adds the node to the node graph and returns it's assigned ID
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public int AddNode(Node node)
	{
		NodeDict[_lastId] = node;
		_lastId++;
		return _lastId - 1;
	}

	public object Clone()
	{
		var json = JsonUtility.ToJson((Serializable_NodeGraph)this);
		return (NodeGraph)JsonUtility.FromJson<Serializable_NodeGraph>(json);
	}

	/// <summary>
	/// registers a new connection from the node pointed to by <paramref name="nodeId0"/> to <paramref name="nodeId1"/>
	/// </summary>
	/// <param name="nodeId0"></param>
	/// <param name="nodeId1"></param>
	public void Connect(int nodeId0, int nodeId1)
	{
		if (nodeId0 != nodeId1 && !NodeDict[nodeId0].ConnectedNodes.Contains(nodeId1))
		{
			NodeDict[nodeId0].ConnectedNodes.Add(nodeId1);
		}
	}

	/// <summary>
	/// adds a connection from <paramref name="node0"/> to <paramref name="node1"/>
	/// </summary>
	/// <param name="node0"></param>
	/// <param name="node1"></param>
	public void Connect(Node node0, Node node1)
	{
		if (node0 == null || node1 == null)
		{
			return;
		}

		var id0 = GetIdFromNode(node0);
		var id1 = GetIdFromNode(node1);
		Connect(id0, id1);
	}

	public void Delete(Node node)
	{
		Disconnect(node);
		NodeDict.Remove(GetIdFromNode(node));
	}

	public void Disconnect(Node disconnect_node)
	{
		var id = GetIdFromNode(disconnect_node);
		foreach (var node in NodeDict)
		{
			if (node.Value.ConnectedNodes.Contains(id))
			{
				node.Value.ConnectedNodes.Remove(id);
			}
		}
	}

	public int GetIdFromNode(Node node0)
	{
		return NodeDict.FirstOrDefault(kv => kv.Value == node0).Key;
	}

	internal void Modify(int nodeID, string newName)
	{
		if (NodeDict.TryGetValue(nodeID, out Node node))
		{
			node.Node_text = newName;
		}
	}

#if UNITY_EDITOR

	/// <summary>
	/// returns a node if one contains the specified position
	/// </summary>
	/// <param name="mousePosition">position to check</param>
	/// <returns></returns>
	public Node GetNodeUnderPosition(Vector2 mousePosition)
	{
		foreach (var item in NodeDict)
		{
			if (item.Value != null)
			{
				if (item.Value.RectContains(mousePosition))
				{
					return item.Value;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// moves all contained nodes by a certain offset amount
	/// </summary>
	/// <param name="offset">amount to move by</param>
	public void Offset(Vector2 offset)
	{
		foreach (var item in NodeDict)
		{
			if (item.Value != null)
			{
				item.Value.Pos += offset;
			}
		}
	}

	/// <summary>
	/// draws the nodes onto the editor window
	/// </summary>
	public void Draw(float scale)
	{
		// we draw the connections first so they render under the nodes
		foreach (var item in NodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}
			DrawConnections(item.Value, scale);
		}

		// then draw the nodes
		foreach (var item in NodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}
			item.Value.Draw(item.Key, scale);
		}
	}

	/// <summary>
	/// draws the bezier curve starting from the right side of this node to all the connected nodes and places a little circle at the end
	/// </summary>
	/// <param name="node"></param>
	private void DrawConnections(Node node, float scale)
	{
		foreach (var connection in node.ConnectedNodes)
		{
			if (NodeDict.TryGetValue(connection, out Node connectedNode) && connectedNode != null)
			{
				// we add the margin because the nodes are rounded
				var start = (node.Pos + new Vector2(_nodeSize - _nodeMargin, _nodeSize * 0.5f)) * scale;
				var end = (connectedNode.Pos + new Vector2(_nodeMargin, _nodeSize * 0.5f)) * scale;
				Vector2 tangentCurve = (Vector2.left * 80 * scale);
				UnityEditor.Handles.DrawBezier(
					start,
					end,
					start - tangentCurve,
					end + tangentCurve,
					color: Color.black,
					texture: null,
					_lineWidth
					);
				UnityEditor.Handles.DrawSolidDisc(end, Vector3.forward, _ballSize * scale);
			}
		}
	}

#endif
}

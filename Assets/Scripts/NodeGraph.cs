using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/// <summary>
/// a class used storing nodes and their relations as well as providing all neccesairy methods for controlling them from the editor window
/// </summary>
public class NodeGraph : ICloneable
{
	// the width of the connection lines
	private const float _lineWidth = 2f;

	// size of the nodes
	// TODO don't hardcode this
	private const int _nodeSize = 100;
	// size of the indication at the end of a connection
	private const float _ballSize = 10;
	// margin around the nodes 
	private const int _nodeMargin = 6;
	// last ID dispatched to a node
	public int _lastId;
	// dictionary storing all nodes by their IDs
	public Dictionary<int, Node> _nodeDict = new Dictionary<int, Node>();

	/// <summary>
	/// adds the node to the nodegraph and returns it's assigned ID
	/// </summary>
	/// <param name="node"></param>
	/// <returns></returns>
	public int CreateNode(Node node)
	{
		_nodeDict[_lastId] = node;
		_lastId++;
		return _lastId - 1;
	}
	/// <summary>
	/// registers a new connection from the node pointed to by <paramref name="nodeId0"/> to <paramref name="nodeId1"/>
	/// </summary>
	/// <param name="nodeId0"></param>
	/// <param name="nodeId1"></param>
	public void Connect(int nodeId0, int nodeId1)
	{
		if (nodeId0 != nodeId1 && !_nodeDict[nodeId0].ConnectedNodes.Contains(nodeId1))
		{
			_nodeDict[nodeId0].ConnectedNodes.Add(nodeId1);
		}
	}

	/// <summary>
	/// moves all contained nodes by a certain offset amount
	/// </summary>
	/// <param name="offset">amount to move by</param>
	public void Offset(Vector2 offset)
	{
		foreach (var item in _nodeDict)
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
	public void Draw()
	{
		// we draw the connections first so they render under the nodes
		foreach (var item in _nodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}
			DrawConnections(item.Value);
		}

		// then draw the nodes
		foreach (var item in _nodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}
			item.Value.Draw(item.Key);
		}
	}

	public void Disconnect(Node disconnect_node)
	{
		var id = GetId(disconnect_node);
		foreach (var node in _nodeDict)
		{
			if (node.Value.ConnectedNodes.Contains(id))
			{
				node.Value.ConnectedNodes.Remove(id);
			}
		}
	}

	public void Delete(Node node)
	{
		Disconnect(node);
		_nodeDict.Remove(GetId(node));
		
	}

	/// <summary>
	/// draws the bezier curve starting from the right side of this node to all the connected nodes and places a little circle at the end
	/// </summary>
	/// <param name="node"></param>
	private void DrawConnections(Node node)
	{
		foreach (var connection in node.ConnectedNodes)
		{
			if (_nodeDict.TryGetValue(connection, out Node connectedNode) && connectedNode != null)
			{
				// we add the margin because the nodes are rounded
				var start = node.Pos + new Vector2(_nodeSize - _nodeMargin, _nodeSize * 0.5f);
				var end = connectedNode.Pos + new Vector2(_nodeMargin, _nodeSize * 0.5f);
				Vector2 tangentCurve = (Vector2.left * 80);
				Handles.DrawBezier(
					start,
					end,
					start - tangentCurve,
					end + tangentCurve,
					color: Color.black,
					texture: null,
					_lineWidth
					);
				Handles.DrawSolidDisc(end, Vector3.forward, _ballSize);
			}
		}
	}
	/// <summary>
	/// returns a node if one contains the specified position
	/// </summary>
	/// <param name="mousePosition">position to check</param>
	/// <returns></returns>
	public Node GetNodeUnderPosition(Vector2 mousePosition)
	{
		foreach (var item in _nodeDict)
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

		var id0 = GetId(node0);
		var id1 = GetId(node1);
		Connect(id0, id1);
	}

	

	private int GetId(Node node0)
	{
		return _nodeDict.FirstOrDefault(kv => kv.Value == node0).Key;
	}

	internal void Modify(int nodeID, string newName)
	{
		if (_nodeDict.TryGetValue(nodeID, out Node node))
		{
			node.Node_text = newName;
		}
	}

	public object Clone()
	{
		var json = JsonUtility.ToJson((Serializable_NodeGraph)this);
		return (NodeGraph)JsonUtility.FromJson<Serializable_NodeGraph>(json);
	}

	public static explicit operator NodeGraph(Serializable_NodeGraph v)
	{
		var dict = new Dictionary<int, Node>();
		for (int i = 0; i < v.Keys.Count; i++)
		{
			dict.Add(v.Keys[i], v.Values[i]);
		}

		return new NodeGraph()
		{
			_nodeDict = dict,
			_lastId = v.lastID
		};
	}
}

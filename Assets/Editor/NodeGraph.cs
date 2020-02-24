using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal class NodeGraph
{
	private const float _lineWidth = 2f;
	private const int _nodeSize = 100;
	private int _lastId;
	private Dictionary<int, Node> _nodeDict = new Dictionary<int, Node>();
	private float _ballSize = 10;
	private int _nodeMargin = 6;

	public NodeGraph()
	{
	}

	public int CreateNode(Node node)
	{
		_nodeDict[_lastId] = node;
		_lastId++;
		return _lastId - 1;
	}
	public void Connect(int nodeId0, int nodeId1)
	{
		_nodeDict[nodeId0].ConnectedNodes.Add(nodeId1);
	}

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

	public void Draw()
	{

		foreach (var item in _nodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}

			DrawConnections(item.Value);
		}
		foreach (var item in _nodeDict)
		{
			if (item.Value == null)
			{
				continue;
			}

			item.Value.Draw();
		}
	}

	private void DrawConnections(Node node)
	{
		foreach (var connection in node.ConnectedNodes)
		{
			if (_nodeDict.TryGetValue(connection, out Node connectedNode) && connectedNode != null)
			{
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

	internal Node GetNodeUnderPosition(Vector2 mousePosition)
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

	internal void Connect(Node node0, Node node1)
	{
		var id = _nodeDict.FirstOrDefault(kv => kv.Value == node1).Key;
		node0.ConnectedNodes.Add(id);
	}
}

﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// basic node class used for drawing in the editor window
/// </summary>
[Serializable]
public class Node
{
	public List<int> _connectedNodes = new List<int>();

	public string _node_text;

	public Vector2 _pos;
	public float _value = 0;
	private const int _defaultFontSize = 12;

	// width and height of the square node
	private const int _nodeSize = 100;

	private static Dictionary<string, Texture> _iconTextures;

	// this is used to control the rounded corners of the node
	private readonly int _boxMargin = 12;

	private GUIStyle _nodeStyle;
	private Rect _rect;

	public Node()
	{
		_rect = new Rect(Pos.x, Pos.y, _nodeSize, _nodeSize);
	}

	public bool Active { get; internal set; }

	/// <summary>
	/// Ids of the corresponding connections in the node graph
	/// </summary>
	public List<int> ConnectedNodes { get => _connectedNodes; set => _connectedNodes = value; }

	/// <summary>
	/// text displayed on the node
	/// </summary>
	public string Node_text { get => _node_text; set => _node_text = value; }

	/// <summary>
	/// position of the node in the editor window in window space
	/// </summary>
	public Vector2 Pos { get => _pos; set => _pos = value; }

	public float Value { get => _value; set => _value = value; }
#if UNITY_EDITOR
	public static Dictionary<string, Texture> IconTextures { get => _iconTextures ?? LoadTexture(); set => _iconTextures = value; }

	/// <summary>
	/// draws the node upon the editor window
	/// </summary>
	internal void Draw(int id, float scale)
	{
		_nodeStyle = _nodeStyle ?? GenerateNodeStyle();
		_nodeStyle.fontSize = (int)(_defaultFontSize * scale);
		_rect.position = Pos * scale;
		_rect.size = new Vector2(_nodeSize, _nodeSize) * scale;

		_nodeStyle.border = new RectOffset((int)(_boxMargin * scale), (int)(_boxMargin * scale), (int)(_boxMargin * scale), (int)(_boxMargin * scale));
		_nodeStyle.padding = _nodeStyle.border;// new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		var text = $"{Node_text}\nid:{id}";
		if (Value != 0)
		{
			text += $"\nvalue:{Value}";
		}

		if (IconTextures.TryGetValue(_node_text, out Texture texture))
		{
			_nodeStyle.normal.background = texture as Texture2D;
			//GUI.DrawTexture(_rect, text, texture);
		}
		GUI.Box(_rect, text, _nodeStyle);
	}

	/// <summary>
	/// returns true if the position is inside the node rectangle
	/// </summary>
	/// <param name="mousePosition"></param>
	/// <returns></returns>
	internal bool RectContains(Vector2 mousePosition)
	{
		return _rect.Contains(mousePosition);
	}

	private static Dictionary<string, Texture> LoadTexture()
	{
		_iconTextures = new Dictionary<string, Texture> {
			{"backGround",  UnityEditor.EditorGUIUtility.Load("simple.png") as Texture},
		};
		foreach (var nodetype in NodeTypes.Types)
		{
			if (nodetype.EditorImage != null)
			{
				_iconTextures.Add(nodetype.Tag, UnityEditor.EditorGUIUtility.Load(nodetype.EditorImage) as Texture);
			}
		}
		return _iconTextures;
	}

	private GUIStyle GenerateNodeStyle()
	{
		var nodeStyle = new GUIStyle();
		nodeStyle.normal.background = IconTextures["backGround"] as Texture2D;
		nodeStyle.wordWrap = true;
		return nodeStyle;
	}

#endif
}
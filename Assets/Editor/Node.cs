using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Node
{
	private const int _nodeSize = 100;
	private GUIStyle _nodeStyle;
	private int _boxMargin = 12;
	private Rect _rect;

	public Vector2 Pos { get; set; }
	public string Node_text { get; set; }
	public HashSet<int> ConnectedNodes { get; set; } = new HashSet<int>();

	public Node(Vector2 pos, string text)
	{
		Pos = pos;
		Node_text = text;

		_nodeStyle = new GUIStyle();
		_nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
		_nodeStyle.border = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		_nodeStyle.padding = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		_nodeStyle.wordWrap = true;
		_rect = new Rect(Pos.x, Pos.y, _nodeSize, _nodeSize);
	}

	internal void Draw()
	{
		_rect.position = Pos;
		GUI.Box(_rect, Node_text, _nodeStyle);
	}

	internal bool RectContains(Vector2 mousePosition)
	{
		return _rect.Contains(mousePosition);
	}
}
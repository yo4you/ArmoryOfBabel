using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// basic node class used for drawing in the editor window
/// </summary>
public class Node
{
	// width and height of the square node
	private const int _nodeSize = 100;
	private GUIStyle _nodeStyle;
	// this is used to control the rounded corners of the node
	private int _boxMargin = 12;
	private Rect _rect;

	/// <summary>
	/// position of the node in the editor window in window space
	/// </summary>
	public Vector2 Pos { get; set; }
	/// <summary>
	/// text displayed on the node
	/// </summary>
	public string Node_text { get; set; }
	/// <summary>
	/// Ids of the corresponding connections in the nodegraph
	/// </summary>
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

	/// <summary>
	/// returns true if the position is inside the node rectangle
	/// </summary>
	/// <param name="mousePosition"></param>
	/// <returns></returns>
	internal bool RectContains(Vector2 mousePosition)
	{
		return _rect.Contains(mousePosition);
	}
}
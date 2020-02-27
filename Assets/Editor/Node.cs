using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// basic node class used for drawing in the editor window
/// </summary>
[Serializable]
public class Node
{
	// width and height of the square node
	private const int _nodeSize = 100;
	private GUIStyle _nodeStyle;
	// this is used to control the rounded corners of the node
	private int _boxMargin = 12;
	private Rect _rect;
	public Vector2 _pos;
	public string _node_text;
	public List<int> _connectedNodes = new List<int>();

	/// <summary>
	/// position of the node in the editor window in window space
	/// </summary>
	public Vector2 Pos { get => _pos; set => _pos = value; }
	/// <summary>
	/// text displayed on the node
	/// </summary>
	public string Node_text { get => _node_text; set => _node_text = value; }
	/// <summary>
	/// Ids of the corresponding connections in the nodegraph
	/// </summary>
	public List<int> ConnectedNodes { get => _connectedNodes; set => _connectedNodes = value; }
	public Node()
	{
		_nodeStyle = new GUIStyle();
		_nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
		_nodeStyle.border = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		_nodeStyle.padding = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		_nodeStyle.wordWrap = true;
		_rect = new Rect(Pos.x, Pos.y, _nodeSize, _nodeSize);
	}

	internal void DoubleClick()
	{
		var windowContent = EditorWindow.CreateInstance<RenameWindow>();
		windowContent.Node = this;
		var pos = Pos + EditorWindow.focusedWindow.position.position;
		windowContent.position = new Rect(pos.x, pos.y, 300, 50);
		windowContent.ShowPopup();
	}

	/// <summary>
	/// draws the node upon the editorwindow
	/// </summary>
	internal void Draw(int id)
	{
		_rect.position = Pos;
		GUI.Box(_rect, $"{Node_text}:{id}", _nodeStyle);
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
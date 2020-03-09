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
	public List<int> _connectedNodes = new List<int>();

	public string _node_text;

	public Vector2 _pos;

	// width and height of the square node
	private const int _nodeSize = 100;

	private static Dictionary<string, Texture> _iconTextures;

	// this is used to control the rounded corners of the node
	private int _boxMargin = 12;

	private GUIStyle _nodeStyle;
	private Rect _rect;
	private Texture _texture;

	public Node()
	{
		_rect = new Rect(Pos.x, Pos.y, _nodeSize, _nodeSize);
	}

	public static Dictionary<string, Texture> IconTextures { get => _iconTextures ?? LoadTexture(); set => _iconTextures = value; }

	/// <summary>
	/// Ids of the corresponding connections in the nodegraph
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

	/// <summary>
	/// draws the node upon the editorwindow
	/// </summary>
	internal void Draw(int id)
	{
		_nodeStyle = _nodeStyle ?? GenerateNodeStyle();
		_rect.position = Pos;
		GUI.Box(_rect, $"{Node_text}:{id}", _nodeStyle);
		if (IconTextures.TryGetValue(_node_text, out Texture texture))
		{
			GUI.DrawTexture(_rect, texture);
		}
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
			{"A",  EditorGUIUtility.Load("BtnA.png") as Texture},
			{"B",  EditorGUIUtility.Load("BtnB.png") as Texture},
			{"X",  EditorGUIUtility.Load("BtnX.png") as Texture},
			{"R",  EditorGUIUtility.Load("BtnR.png") as Texture},
			{"OUT",  EditorGUIUtility.Load("BtnOut.png") as Texture},
			{"VAL",  EditorGUIUtility.Load("BtnVal.png") as Texture},
			{"OR",  EditorGUIUtility.Load("BtnOR.png") as Texture},
			{"AND",  EditorGUIUtility.Load("BtnAnd.png") as Texture},
			{"NOT",  EditorGUIUtility.Load("BtnNot.png") as Texture},
			{"UI",  EditorGUIUtility.Load("BtnUI.png") as Texture},
			{"HIT",  EditorGUIUtility.Load("BtnHit.png") as Texture},
			{"TYPE",  EditorGUIUtility.Load("Settings.png") as Texture},
			{"SPD",  EditorGUIUtility.Load("menu.png") as Texture},
			{"DMG",  EditorGUIUtility.Load("starRate.png") as Texture},
		};
		return _iconTextures;
	}

	private GUIStyle GenerateNodeStyle()
	{
		var nodeStyle = new GUIStyle();
		nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D;
		nodeStyle.border = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		nodeStyle.padding = new RectOffset(_boxMargin, _boxMargin, _boxMargin, _boxMargin);
		nodeStyle.wordWrap = true;
		return nodeStyle;
	}
}

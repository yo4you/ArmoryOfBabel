using System;
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
			{"A",   UnityEditor.EditorGUIUtility.Load("BtnA.png") as Texture},
			{"B",   UnityEditor.EditorGUIUtility.Load("BtnB.png") as Texture},
			{"X",   UnityEditor.EditorGUIUtility.Load("BtnX.png") as Texture},
			{"R",   UnityEditor.EditorGUIUtility.Load("BtnR.png") as Texture},
			{"OUT", UnityEditor.EditorGUIUtility.Load("BtnOut.png") as Texture},
			{"VAL", UnityEditor.EditorGUIUtility.Load("BtnVal.png") as Texture},
			{"OR",  UnityEditor.EditorGUIUtility.Load("BtnOR.png") as Texture},
			{"AND", UnityEditor.EditorGUIUtility.Load("BtnAnd.png") as Texture},
			{"NOT", UnityEditor.EditorGUIUtility.Load("BtnNot.png") as Texture},
			{"UI",  UnityEditor.EditorGUIUtility.Load("BtnUI.png") as Texture},
			{"HIT", UnityEditor.EditorGUIUtility.Load("BtnHit.png") as Texture},
			{"TYPE",UnityEditor.EditorGUIUtility.Load("Settings.png") as Texture},
			{"SPD", UnityEditor.EditorGUIUtility.Load("menu.png") as Texture},
			{"DMG", UnityEditor.EditorGUIUtility.Load("starRate.png") as Texture},
			{"AH",  UnityEditor.EditorGUIUtility.Load("BtnAh.png") as Texture},
			{"BH",  UnityEditor.EditorGUIUtility.Load("BtnBh.png") as Texture},
			{"XH",  UnityEditor.EditorGUIUtility.Load("BtnXh.png") as Texture},
			{"UIC", UnityEditor.EditorGUIUtility.Load("BtnUIcap.png") as Texture},
			{"DT",  UnityEditor.EditorGUIUtility.Load("dt.png") as Texture},
			{"COPY",UnityEditor.EditorGUIUtility.Load("BtnCopy.png") as Texture},
			{"SUM", UnityEditor.EditorGUIUtility.Load("BtnSUM.png") as Texture},
			{"MOV", UnityEditor.EditorGUIUtility.Load("knob.png") as Texture},
			{"STAT", UnityEditor.EditorGUIUtility.Load("stat.png") as Texture},
		};
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

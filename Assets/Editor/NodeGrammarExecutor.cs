﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeGrammarExecutor : EditorWindow
{
	private const float _buttonWidth = 20;

	// ratio * w = h
	private const float _editorWHRatio = 0.75f;

	// width * marginratio = margin
	private const float _marginRatio = 0.02f;

	private readonly float _editorHeightRatio = 0.5f;
	private readonly NodeGraph[] _nodegraphs = new NodeGraph[2] { null, null };
	private bool _enabledLastFrame;
	private Vector2 _grammarViewScroll;
	private bool _inputDirty;
	private string _inputString;
	private string _nodeDirectory = "";
	private NodeEditorWindow[] _nodeEditorWindows = new NodeEditorWindow[] { null, null };
	private bool _nodeGrammarDirty;
	private string _nodeGrammarName;
	private List<NodeGrammar> _nodeGrammars = new List<NodeGrammar>();
	private string _outputString;
	private int _seed;
	private bool _selected = true;
	private string _stringDirectory = "";
	private bool _stringGrammarDirty;
	private string _stringGrammarName;
	private List<StringGrammarRule> _stringgrammars;

	private enum HandSide { LEFT, RIGHT }

	public string InputString
	{
		get => _inputString; set
		{
			if (value != _inputString)
			{
				_inputDirty = true;
			}

			_inputString = value;
		}
	}

	public int Seed
	{
		get => _seed; set
		{
			if (_seed != value)
			{
				_inputDirty = true;
			}
			_seed = value;
		}
	}

	public string StringGrammarName
	{
		get => _stringGrammarName; set
		{
			if (_stringGrammarName != value)
			{
				_stringGrammarDirty = true;
				_inputDirty = true;
			}
			_stringGrammarName = value;
		}
	}

	[MenuItem("Custom/Node Grammar Executor")]
	private static void OpenWindow()
	{
		var window = GetWindow<NodeGrammarExecutor>(utility: false, "Node Grammar Executor", focus: true);
		// this ensures we can get notified when the user clicks away from the editor window
		window.wantsMouseEnterLeaveWindow = true;
	}

	private void DrawEditorWindows()
	{
		var margin = _marginRatio * position.width;
		float editorWidth = (position.width - 3 * margin) / 2;
		float editorHeight = position.height * _editorHeightRatio;
		float editorYOffset = position.height - editorHeight - margin;
		for (int i = 0; i < 2; i++)
		{
			_nodeEditorWindows[i].position = new Rect(
			position.position.x + margin + (i == (int)HandSide.RIGHT ? editorWidth + margin : 0),
			position.position.y + editorYOffset,
			editorWidth,
			editorHeight);
		}
	}

	private void DrawNodeOptions()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Node Grammar Name");
		_nodeGrammarName = GUILayout.TextField(_nodeGrammarName);
		if (GUILayout.Button("+", GUILayout.Width(_buttonWidth)))
		{
			var grammars = NodeGrammar.ImportGrammars(_nodeDirectory + _nodeGrammarName + ".json");
			_nodeGrammars.AddRange(grammars);
			_nodeGrammarDirty = true;
		}
		if (GUILayout.Button("↺", GUILayout.Width(_buttonWidth)))
		{
			_nodeGrammars = new List<NodeGrammar>();
			_nodeGrammarDirty = true;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField("Loaded node grammars");
		_grammarViewScroll = EditorGUILayout.BeginScrollView(_grammarViewScroll, GUILayout.Height(100f));
		foreach (var grammar in _nodeGrammars)
		{
			EditorGUILayout.LabelField(grammar.Name);
		}
		EditorGUILayout.EndScrollView();

		if (_nodeEditorWindows[0].Changed || _nodeGrammarDirty)
		{
			_nodeGrammarDirty = false;
			_nodeEditorWindows[0].Changed = false;
			_nodegraphs[0] = _nodeEditorWindows[0].Nodegraph;
			_nodegraphs[1] = GrammarUtils.ApplyNodeGrammars(_outputString, ref _nodeGrammars, _nodegraphs[0], Seed);
			_nodeEditorWindows[1].Nodegraph = _nodegraphs[1];
		}
	}

	private void DrawResetButtom()
	{
		if (GUILayout.Button("Refresh"))
		{
			_stringGrammarDirty = true;
			_nodeGrammarDirty = true;
		}
	}

	private void DrawStringGrammarOptions()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("String Grammar Import :");
		StringGrammarName = EditorGUILayout.TextField(StringGrammarName);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Input String :");
		InputString = GUILayout.TextField(InputString);
		EditorGUILayout.LabelField("Seed :");
		Seed = EditorGUILayout.IntField(Seed);
		EditorGUILayout.EndHorizontal();

		if (_stringGrammarDirty)
		{
			_stringgrammars = GrammarUtils.ImportGrammars(_stringDirectory + StringGrammarName + ".json");
			_stringGrammarDirty = false;
		}
		if (_inputDirty)
		{
			_inputDirty = false;
			if (_stringgrammars != null)
			{
				_outputString = GrammarUtils.ApplyGrammars(ref _stringgrammars, InputString, Seed);
			}
		}
		EditorGUILayout.LabelField($"Output String : {_outputString}");
		EditorGUILayout.Space();
	}

	private void Enable()
	{
		GenerateEditorWindows();
		_selected = true;
	}

	private void GenerateEditorWindows()
	{
		foreach (var window in _nodeEditorWindows)
		{
			window?.Close();
		}
		_nodeEditorWindows = new NodeEditorWindow[2];
		for (int i = 0; i < 2; i++)
		{
			_nodeEditorWindows[i] = CreateInstance<NodeEditorWindow>();
			_nodeEditorWindows[i].ShowPopup();
			_nodeEditorWindows[i].Nodegraph = new NodeGraph();
		}
		_nodeEditorWindows[1].Editable = false;
	}

	private void OnDisable()
	{
		foreach (var item in _nodeEditorWindows)
		{
			if (item != null)
			{
				item.CloseNextFrame = true;
			}
		}
	}

	private void OnEnable()
	{
		_enabledLastFrame = true;
	}

	private void OnFocus()
	{
		if (!_selected)
		{
			_selected = true;
			GenerateEditorWindows();
		}
		for (int i = 0; i < 2; i++)
		{
			//_nodeEditorWindows[i].Nodegraph = _nodegraphs[i];
		}
	}

	private void OnGUI()
	{
		if (_enabledLastFrame)
		{
			Enable();
			_enabledLastFrame = false;
			return;
		}
		if (_nodeDirectory == "")
		{
			_nodeDirectory = Application.streamingAssetsPath + "/Grammar/Node/";
			_stringDirectory = Application.streamingAssetsPath + "/Grammar/String/";
		}

		ResetWindowButton();
		DrawStringGrammarOptions();
		DrawNodeOptions();
		DrawResetButtom();
		if (_selected)
		{
			DrawEditorWindows();
		}
	}

	private void OnLostFocus()
	{
		// when the window stops being visible the focused window will be set to null, there's no other way to hide sub windows
		if (focusedWindow == null)
		{
			_selected = false;
			if (_nodeEditorWindows[0] == null)
			{
				return;
			}
			SaveNodeGraphs();

			for (int i = 0; i < 2; i++)
			{
				_nodegraphs[i] = _nodeEditorWindows[i].Nodegraph;
				_nodeEditorWindows[i].Close();
			}
		}
	}

	private void ResetWindowButton()
	{
		if (GUILayout.Button("Reset the Windows"))
		{
			for (int i = 0; i < 4; i++)
			{
				GetWindow<NodeEditorWindow>()?.Close();
			}
			GenerateEditorWindows();
		}
	}

	private void SaveNodeGraphs()
	{
	}
}
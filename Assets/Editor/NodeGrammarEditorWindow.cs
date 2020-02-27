using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NodeGrammarEditorWindow : EditorWindow
{
	// width * ratio = margin
	private const float _marginRatio = 0.02f;
	// ratio * w = h
	private const float _editorWHRatio = 0.75f;
	private NodeGrammarEditorWindow _window;
	private NodeEditorWindow[] _nodeEditorWindows;
	private float _editorHeightRatio = 0.75f;
	private bool _selected = true;
	private bool _enabledLastFrame;
	private List<NodeGrammar> _grammars;
	private int _grammarSelectedIndex;
	private float _buttonWidth = 20;
	private Vector2 _scrollPos;
	private string _exportName;
	private string _directory = "";

	private enum HandSide { LEFT, RIGHT }


	[MenuItem("Custom/Node Grammar Editor")]
	private static void OpenWindow()
	{
		var window = GetWindow<NodeGrammarEditorWindow>(utility: false, "Node Grammar Editor", focus: true);
		// this ensures we can get notified when the user clicks away from the editor window
		window.wantsMouseEnterLeaveWindow = true;
	}

	private void OnEnable()
	{
		// for some reason OnEnable is called when starting play mode but OnGui isn't called so we get weird floating windows
		_enabledLastFrame = true;
	}

	private void EnableProperly()
	{
		_grammars = new List<NodeGrammar>
		{
			new NodeGrammar
			{
				Name = "New Grammar",
				LeftHand = new NodeGraph(),
				RightHand = new NodeGraph()
			}
		};
		GenerateEditorWindows();
		_selected = true;
	}

	private void OnLostFocus()
	{
		// when the window stops being visible the focused window will be set to null, there's no other way to hide subwindows
		if (focusedWindow == null)
		{
			if (_nodeEditorWindows[0] == null)
			{
				return;
			}
			_selected = false;
			SaveGrammar(_grammarSelectedIndex);

			for (int i = 0; i < 2; i++)
			{
				_nodeEditorWindows[i].Close();
			}
		}
	}

	private void SaveGrammar(int grammarIndex)
	{
		if (_grammars.Count == 0)
		{
			return;
		}

		_grammars[grammarIndex] = new NodeGrammar
		{
			Name = _grammars[grammarIndex].Name,
			LeftHand = _nodeEditorWindows[(int)HandSide.LEFT].Nodegraph,
			RightHand = _nodeEditorWindows[(int)HandSide.RIGHT].Nodegraph
		};
	}

	private void OnFocus()
	{
		if (!_selected)
		{
			_selected = true;
			GenerateEditorWindows();
		}
	}

	private void GenerateEditorWindows()
	{
		_nodeEditorWindows = new NodeEditorWindow[2];
		for (int i = 0; i < 2; i++)
		{
			_nodeEditorWindows[i] = CreateInstance<NodeEditorWindow>();
			_nodeEditorWindows[i].ShowPopup();
		}
		LoadGrammar(_grammarSelectedIndex);

	}

	private void LoadGrammar(int grammarIndex)
	{
		if (_grammars.Count == 0)
		{
			return;
		}

		_nodeEditorWindows[(int)HandSide.LEFT].Nodegraph = _grammars[grammarIndex].LeftHand;
		_nodeEditorWindows[(int)HandSide.RIGHT].Nodegraph = _grammars[grammarIndex].RightHand;
	}
	private void OnDisable()
	{
		if (this == null)
		{
			return;
		}

		foreach (var item in _nodeEditorWindows)
		{
			item?.Close();
		}
	}
	private void OnGUI()
	{
		if (_directory == "")
		{
			_directory = Application.streamingAssetsPath + "/Grammar/Node/";
		}
		if (_enabledLastFrame)
		{
			_enabledLastFrame = false;
			EnableProperly();
			return;
		}
		if (GUILayout.Button("Reset the Windows"))
		{
			for (int i = 0; i < 4; i++)
			{
				GetWindow<NodeEditorWindow>()?.Close();
			}
			GenerateEditorWindows();
		}

		#region Grammar selection
		int current_grammarIndex = _grammarSelectedIndex;
		var grammar_labels = new string[_grammars.Count];
		for (int i = 0; i < _grammars.Count; i++)
		{
			grammar_labels[i] = _grammars[i].Name;
		}

		GUILayout.BeginHorizontal();
		_grammarSelectedIndex = GUILayout.Toolbar(_grammarSelectedIndex, grammar_labels);
		if (GUILayout.Button("+", GUILayout.Width(_buttonWidth)))
		{
			_grammars.Add(new NodeGrammar
			{
				Name = "New Grammar",
				LeftHand = new NodeGraph(),
				RightHand = new NodeGraph()
			});
		}
		GUILayout.EndHorizontal();
		if (current_grammarIndex != _grammarSelectedIndex)
		{
			SaveGrammar(current_grammarIndex);
			LoadGrammar(_grammarSelectedIndex);
		}

		#endregion

		if (_grammars.Count == 0)
		{
			return;
		}
		if (_grammars.Count != 1 && GUILayout.Button("Delete Grammar"))
		{
			_grammars.RemoveAt(_grammarSelectedIndex);
			_grammarSelectedIndex = 0;
			LoadGrammar(_grammarSelectedIndex);
			return;
		}
		#region Grammar name change
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Grammar Name");
		var current_grammar = _grammars[_grammarSelectedIndex];
		current_grammar.Name = GUILayout.TextField(_grammars[_grammarSelectedIndex].Name);
		_grammars[_grammarSelectedIndex] = current_grammar;
		GUILayout.EndHorizontal();
		#endregion

		#region Save and Load

		_exportName = EditorGUILayout.TextField("Grammar Name : ", _exportName);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("import"))
		{
			StreamReader reader = new StreamReader(_directory + _exportName + ".json");
			var jsonString = reader.ReadToEnd();
			_grammars = SerializableNodeGrammars_Converter.FromJson(jsonString);
			reader.Close();
			reader.Dispose();
			LoadGrammar(_grammarSelectedIndex);
		}
		if (GUILayout.Button("export"))
		{
			SaveGrammar(_grammarSelectedIndex);
			StreamWriter writer = new StreamWriter(_directory + _exportName + ".json");
			var jsonString = SerializableNodeGrammars_Converter.ToJson(_grammars);
			writer.Write(jsonString);
			writer.Close();
			writer.Dispose();
		}
		EditorGUILayout.EndHorizontal();
		#endregion



		if (_selected)
		{
			DrawEditorWindows();
		}

		ProcessEvents(Event.current);

		GUIContent content = new GUIContent();
		if (GUI.changed)
		{
			Repaint();
		}
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

	private void ProcessEvents(Event e)
	{
	}
}

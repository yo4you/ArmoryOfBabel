using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class NodeGrammarEditorWindow : EditorWindow
{
	#region Fields

	// ratio * w = h
	private const float _editorWHRatio = 0.75f;

	// width * ratio = margin
	private const float _marginRatio = 0.02f;

	private readonly float _buttonWidth = 20;
	private readonly float _editorHeightRatio = 0.5f;
	private readonly NodeGrammarEditorWindow _window;
	private string _directory = "";
	private bool _enabledLastFrame;
	private string _exportName;
	private List<NodeGrammar> _grammars = new List<NodeGrammar>();
	private int _grammarSelectedIndex;
	private NodeEditorWindow[] _nodeEditorWindows = new NodeEditorWindow[] { null, null };
	private Vector2 _scrollPos;
	private bool _selected = true;

	private enum HandSide { LEFT, RIGHT }

	#endregion Fields

	internal static List<NodeGrammar> ImportGrammars(string directory)
	{
		StreamReader reader = new StreamReader(directory);
		var jsonString = reader.ReadToEnd();
		var outp = SerializableNodeGrammars_Converter.FromJson(jsonString);
		reader.Close();
		reader.Dispose();
		return outp;
	}

	[MenuItem("Custom/Node Grammar Editor")]
	private static void OpenWindow()
	{
		var window = GetWindow<NodeGrammarEditorWindow>(utility: false, "Node Grammar Editor", focus: true);
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
		}
		LoadGrammar(_grammarSelectedIndex);
	}

	private void GrammarNameOptions()
	{
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Grammar Name");
		var current_grammar = _grammars[_grammarSelectedIndex];
		current_grammar.Name = GUILayout.TextField(_grammars[_grammarSelectedIndex].Name);
		_grammars[_grammarSelectedIndex] = current_grammar;
		GUILayout.EndHorizontal();
	}

	private void GrammarSaveAndLoad()
	{
		_exportName = EditorGUILayout.TextField("File Name : ", _exportName);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("import"))
		{
			_grammars = ImportGrammars(_directory + _exportName + ".json");
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
	}

	private void GrammarToolbar()
	{
		int current_grammarIndex = _grammarSelectedIndex;
		var grammar_labels = new string[_grammars.Count];
		for (int i = 0; i < _grammars.Count; i++)
		{
			grammar_labels[i] = _grammars[i].Name;
		}

		EditorGUILayout.BeginHorizontal();
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
		EditorGUILayout.EndHorizontal();
		if (current_grammarIndex != _grammarSelectedIndex)
		{
			SaveGrammar(current_grammarIndex);
			LoadGrammar(_grammarSelectedIndex);
		}
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
		foreach (var item in _nodeEditorWindows)
		{
			item.CloseNextFrame = true;
		}
	}

	private void OnEnable()
	{
		// for some reason OnEnable is called when starting play mode but OnGui isn't called so we get weird floating windows
		_enabledLastFrame = true;
	}

	private void OnFocus()
	{
		if (!_selected)
		{
			_selected = true;
			GenerateEditorWindows();
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

		WindowResetButton();

		GrammarToolbar();

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

		GrammarNameOptions();

		GrammarSaveAndLoad();

		if (_selected)
		{
			DrawEditorWindows();
		}
	}

	private void OnLostFocus()
	{
		// when the window stops being visible the focused window will be set to null, there's no other way to hide subwindows
		if (focusedWindow == null)
		{
			_selected = false;
			if (_nodeEditorWindows[0] == null)
			{
				return;
			}

			SaveGrammar(_grammarSelectedIndex);
			_grammarSelectedIndex = 0;
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

	private void WindowResetButton()
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
}

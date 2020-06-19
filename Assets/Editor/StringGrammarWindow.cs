using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StringGrammarWindow : EditorWindow
{
	private int _bottomPaddingLines = 10;
	private float _buttonWidth = 40;
	private int _chance;
	private string _directory = "";
	private bool _evalDirty = true;
	private List<string> _evals = new List<string>();
	private string _exportName = "";
	private Vector2 _grammarListScroll;
	private List<StringGrammarRule> _grammars = new List<StringGrammarRule>();
	private string _leftHandString = "";
	private int _removedEntryIndex = -1;
	private string _rightHandString = "";
	private int _rowSize = 5;
	private int _testExamples = 20;
	private Vector2 _testListScroll;
	private string _testString = "";
	private float _textBoxSize = 80;

	public List<StringGrammarRule> Grammars
	{
		get => _grammars; set
		{
			_evalDirty = true;
			_grammars = value;
		}
	}

	public string TestString
	{
		get => _testString; set
		{
			if (_testString != value)
			{
				_evalDirty = true;
			}

			_testString = value;
		}
	}

	public static void ExportGrammars(string fileDir, ref List<StringGrammarRule> grammars)
	{
		StreamWriter writer = new StreamWriter(fileDir);
		var jsonString = JsonUtility.ToJson(new SerializableGrammars() { Values = grammars });
		writer.Write(jsonString);
		writer.Close();
		writer.Dispose();
	}

	[MenuItem("Custom/StringGrammarWindow")]
	public static void ShowWindow()
	{
		GetWindow<StringGrammarWindow>(false, "String Grammars", true);
	}

	public void Reset()
	{
		_chance = 0;
		_directory = "";
		_exportName = "";
		_grammarListScroll = new Vector2();
		Grammars = new List<StringGrammarRule>();
		_leftHandString = "";
		_removedEntryIndex = -1;
		_rightHandString = "";
	}

	private void GrammarAddition()
	{
		EditorGUILayout.LabelField("Add string grammar");
		if (_removedEntryIndex != -1)
		{
			Grammars.RemoveAt(_removedEntryIndex);
			_evalDirty = true;
		}
		_removedEntryIndex = -1;

		EditorGUILayout.BeginHorizontal();
		_leftHandString = EditorGUILayout.TextField("", _leftHandString, GUILayout.Width(_textBoxSize));
		EditorGUILayout.LabelField(" → ", GUILayout.Width(20));
		_rightHandString = EditorGUILayout.TextField("", _rightHandString, GUILayout.Width(_textBoxSize));
		GUILayout.FlexibleSpace();
		_chance = EditorGUILayout.IntField("chance", _chance, GUILayout.Width(_textBoxSize * 2.5f));
		if (GUILayout.Button("+", GUILayout.Width(_buttonWidth)))
		{
			Grammars.Add(new StringGrammarRule()
			{
				LeftHand = _leftHandString,
				RightHand = _rightHandString,
				Chance = _chance
			});
			Grammars.OrderBy(x => x.Chance);
			_evalDirty = true;
		}
		EditorGUILayout.EndHorizontal();
	}

	private void GrammarDisplay()
	{
		if (Grammars == null)
		{
			return;
		}

		_grammarListScroll = EditorGUILayout.BeginScrollView(_grammarListScroll);
		for (int i = 0; i < Grammars.Count; i++)
		{
			var grammar = Grammars[i];
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"{grammar.LeftHand}  →  {grammar.RightHand}");
			EditorGUILayout.LabelField($"{grammar.Chance}", GUILayout.Width(_buttonWidth));

			if (GUILayout.Button("-", GUILayout.Width(_buttonWidth)))
			{
				_removedEntryIndex = i;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndScrollView();
	}

	private void GrammarEvaluator()
	{
		EditorGUILayout.LabelField("Grammar Evaluator");

		TestString = EditorGUILayout.TextField("Test String", TestString);
		_testListScroll = EditorGUILayout.BeginScrollView(_testListScroll, GUILayout.Height(_testExamples * _rowSize));

		if (_evalDirty)
		{
			_evals = new List<string>();
			for (int i = 0; i < _testExamples; i++)
			{
				_evals.Add(GrammarUtils.ApplyGrammars(ref _grammars, TestString, i) ?? "");
			}
			_evalDirty = false;
		}

		for (int i = 0; i < _testExamples; i++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(_evals[i]);
			EditorGUILayout.LabelField($"seed : {i}", GUILayout.Width(_textBoxSize));
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();
	}

	private void OnGUI()
	{
		if (_directory == "")
		{
			_directory = Application.streamingAssetsPath + "/Grammar/String/";
		}

		GrammarAddition();

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		GrammarDisplay();

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Separator();

		GrammarEvaluator();

		// TODO find a better way to do this
		for (int i = 0; i < _bottomPaddingLines; i++)
		{
			EditorGUILayout.Separator();
		}

		SaveAndLoadDisplay();
		if (GUILayout.Button("reset"))
		{
			Reset();
		}
	}

	private void SaveAndLoadDisplay()
	{
		_exportName = EditorGUILayout.TextField("Grammar Name : ", _exportName);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("import"))
		{
			Grammars = GrammarUtils.ImportGrammars(_directory + _exportName + ".json") ?? new List<StringGrammarRule>();
		}
		if (GUILayout.Button("export"))
		{
			ExportGrammars(_directory + _exportName + ".json", ref _grammars);
		}
		EditorGUILayout.EndHorizontal();
	}
}
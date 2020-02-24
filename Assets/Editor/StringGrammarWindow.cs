using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class StringGrammarWindow : EditorWindow
{
	private string _leftHandString = "";
	private string _rightHandString = "";
	private int _chance;
	private Vector2 _grammarListScroll;
	private List<StringGrammarRule> _grammars = new List<StringGrammarRule>();
	private int _removedEntryIndex = -1;
	private float _buttonWidth = 40;
	private float _textBoxSize = 80;
	private int _bottomPaddingLines = 10;
	private string _testString = "";
	private Vector2 _testListScroll;
	private int _testExamples = 20;
	private int _rowSize = 5;
	private string _directory = "";
	private string _exportName = "";

	[MenuItem("Custom/StringGrammarWindow")]
	public static void ShowWindow()
	{
		GetWindow<StringGrammarWindow>(false, "String Grammars", true);
	}
	
	private void OnGUI()
	{
		if (_directory == "")
		{
			_directory = Application.streamingAssetsPath + "/Grammar/";
		}
		#region AddGrammars
		EditorGUILayout.LabelField("Add string grammar");
		if (_removedEntryIndex != -1)
		{
			_grammars.RemoveAt(_removedEntryIndex);
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
			_grammars.Add(new StringGrammarRule()
			{
				LeftHand = _leftHandString,
				RightHand = _rightHandString,
				Chance = _chance
			});
			_grammars.OrderBy(x => x.Chance);
		}
		EditorGUILayout.EndHorizontal();

		#endregion

		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		#region GrammarList
		_grammarListScroll = EditorGUILayout.BeginScrollView(_grammarListScroll);
		for (int i = 0; i < _grammars.Count; i++)
		{
			var grammar = _grammars[i];
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
		#endregion

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		EditorGUILayout.Separator();

		#region Tester
		EditorGUILayout.LabelField("Grammar Evaluator");

		_testString = EditorGUILayout.TextField("Test String", _testString);
		_testListScroll = EditorGUILayout.BeginScrollView(_testListScroll, GUILayout.Height(_testExamples * _rowSize));

		for (int i = 0; i < _testExamples; i++)
		{
			var testResult = GrammarUtils.ApplyGrammars(ref _grammars, _testString, i);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(testResult);
			EditorGUILayout.LabelField($"seed : {i}", GUILayout.Width(_textBoxSize));
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		// TODO find a better way to do this
		for (int i = 0; i < _bottomPaddingLines; i++)
		{
			EditorGUILayout.Separator();
		}
		#endregion

		#region Save and Load

		_exportName= EditorGUILayout.TextField("Grammar Name : ", _exportName);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("import"))
		{
			StreamReader reader = new StreamReader(_directory + _exportName + ".json");
			var jsonString = reader.ReadToEnd();
			_grammars = JsonUtility.FromJson<SerializableGrammars>(jsonString).Values;
			reader.Close();
			reader.Dispose();
		}
		if (GUILayout.Button("export"))
		{
			StreamWriter writer = new StreamWriter(_directory + _exportName + ".json");
			var jsonString = JsonUtility.ToJson(new SerializableGrammars() { Values = _grammars });
			writer.Write(jsonString);
			writer.Close();
			writer.Dispose();
		}
		EditorGUILayout.EndHorizontal();
		#endregion
	}
}

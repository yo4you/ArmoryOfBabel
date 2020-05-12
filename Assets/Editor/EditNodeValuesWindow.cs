using UnityEditor;
using UnityEngine;

internal class EditNodeValuesWindow : EditorWindow
{
	private string _newName = "";
	private float _newValue = float.NaN;
	public Node Node { get; set; }

	private void OnGUI()
	{
		if (_newName == "")
		{
			_newName = Node.Node_text;
		}
		if (float.IsNaN(_newValue))
		{
			_newValue = Node.Value;
		}

		_newName = EditorGUILayout.TextField("New Name", _newName);
		_newValue = EditorGUILayout.FloatField("New Value", _newValue);
		if (Event.current.type == EventType.MouseLeaveWindow ||
			Event.current.keyCode == KeyCode.Return ||
			GUILayout.Button("Confirm"))
		{
			Node.Node_text = _newName;

			Node.Value = _newValue;

			Close();
		}
	}
}

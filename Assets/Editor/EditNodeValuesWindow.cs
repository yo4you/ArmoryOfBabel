using UnityEditor;
using UnityEngine;

internal class EditNodeValuesWindow : EditorWindow
{
	private string _newName = "";
	private float _newValue = 0;
	public Node Node { get; set; }

	private void OnGUI()
	{
		_newName = EditorGUILayout.TextField("New Name", _newName);
		_newValue = EditorGUILayout.FloatField("New Value", _newValue);
		if (Event.current.type == EventType.MouseLeaveWindow ||
			Event.current.keyCode == KeyCode.Return ||
			GUILayout.Button("Confirm"))
		{
			// TODO this can be refactored
			if (_newName != "")
			{
				Node.Node_text = _newName;
			}
			if (_newValue != 0)
			{
				Node.Value = _newValue;
			}
			Close();
		}
	}
}

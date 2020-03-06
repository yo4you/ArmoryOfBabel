using UnityEditor;
using UnityEngine;

internal class RenameWindow : EditorWindow
{
	private string _result = "";
	public Node Node { get; set; }

	private void OnGUI()
	{
		_result = EditorGUILayout.TextField("New Name", _result);
		if (Event.current.type == EventType.MouseLeaveWindow ||
			Event.current.keyCode == KeyCode.Return ||
			GUILayout.Button("Confirm"))
		{
			// TODO this can be refactored
			if (_result != "")
			{
				Node.Node_text = _result;
			}
			Close();
		}
	}
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


/// <summary>
/// an editor window used for creating basic nodegraphs
/// </summary>
public class NodeEditorWindow : EditorWindow
{
	private NodeGraph _nodegraph;
	private int _nodeCount = 20;
	private WindowState _currentState = WindowState.SELECTED;
	/// <summary>
	/// the object we last clicked
	/// </summary>
	private Node _selectedObject;
	
	private enum WindowState
	{
		UNSELECTED,
		SELECTED,
		CLICKED,
		RIGHT_CLICKED
	}

	[MenuItem("Custom/Node Based Editor")]
	private static void OpenWindow()
	{
		var window = GetWindow<NodeEditorWindow>(false, "Node Editor", true);
		// this ensures we can get notified when the user clicks away from the editor window
		window.wantsMouseEnterLeaveWindow = true;
	}

	public void OnEnable()
	{
		_nodegraph = new NodeGraph();
		List<int> nodeIDs = new List<int>();
		// just generated some random nodes for now
		for (int i = 0; i < _nodeCount; i++)
		{
			var pos = Random.insideUnitCircle;
			pos *= Random.Range(0, 1000);
			nodeIDs.Add(_nodegraph.CreateNode(new Node(pos, "A")));
		}
	}

	private void OnGUI()
	{
		_nodegraph.Draw();
		ProcessEvents(Event.current);

		if (GUI.changed)
		{
			Repaint();
		}
	}

	
	private void ProcessEvents(Event e)
	{
		// check if the player clicks away from the window and when they click back
		switch (e.type)
		{
			case EventType.MouseEnterWindow:
				_currentState = WindowState.SELECTED;
				break;
			case EventType.MouseLeaveWindow:
				_currentState = WindowState.UNSELECTED;
				_selectedObject = null;
				break;
			default:
				break;
		}

		switch (_currentState)
		{
			case WindowState.RIGHT_CLICKED:
			case WindowState.SELECTED:
				if (e.type == EventType.MouseDown)
				{
					Click(e.mousePosition, e.button);
				}
				break;
			case WindowState.CLICKED:
				// escape will put us back in a starting state at any time 
				if (e.type == EventType.MouseUp || (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape))
				{
					_currentState = WindowState.SELECTED;
					_selectedObject = null;
				}
				if (e.type == EventType.MouseDrag)
				{
					Drag(e.delta);
					// this ensures we drag each frame instead of only when we move the  mouse
					e.Use();
				}
				break;
			case WindowState.UNSELECTED:
			default:
				break;
		}
	}

	/// <summary>
	/// moves the selected object or the screen if the background is selected
	/// </summary>
	/// <param name="delta"></param>
	private void Drag(Vector2 delta)
	{
		if (_selectedObject != null)
		{
			_selectedObject.Pos += delta;
		}
		else
		{
			_nodegraph.Offset(delta);
		}
	}

	/// <summary>
	/// stores the object, if any that falls under the position <paramref name="mousePosition"/>
	/// will connect them to any subsequent object if right clicked
	/// </summary>
	/// <param name="mousePosition"></param>
	/// <param name="button"></param>
	private void Click(Vector2 mousePosition, int button)
	{
		var clicked_object = _nodegraph.GetNodeUnderPosition(mousePosition);
		switch (button)
		{
			case 0:
				_selectedObject = clicked_object;
				_currentState = WindowState.CLICKED;
				break;
			case 1:
				if (_selectedObject != null && _currentState == WindowState.RIGHT_CLICKED)
				{
					_nodegraph.Connect(_selectedObject, clicked_object);
					_currentState = WindowState.SELECTED;
					_selectedObject = null;
					GUI.changed = true;
				}
				else
				{
					_currentState = WindowState.RIGHT_CLICKED;
					_selectedObject = clicked_object;
				}
				break;
			default:
				break;
		}
	}
}

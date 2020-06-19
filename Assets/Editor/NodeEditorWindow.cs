using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// an editor window used for creating basic nodegraphs
/// </summary>
public class NodeEditorWindow : EditorWindow
{
	private readonly float _zoomMax = 10f;

	private readonly float _zoomSpeed = 0.1f;

	private Dictionary<string, string> _addNodeOptions = new Dictionary<string, string>();
	private WindowState _currentState = WindowState.SELECTED;
	private NodeGraph _nodegraph;
	private bool _repaint;

	/// <summary>
	/// the object we last clicked
	/// </summary>
	private Node _selectedObject;

	private float _zoomAmount = 1f;

	private enum WindowState
	{
		UNSELECTED,
		SELECTED,
		CLICKED,
		RIGHT_CLICKED
	}

	public bool Changed { get; internal set; }

	public bool CloseNextFrame { get; internal set; }

	public bool Editable { get; internal set; } = true;

	public NodeGraph Nodegraph
	{
		get => _nodegraph; set
		{
			_repaint = true;
			_nodegraph = value;
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
		var clicked_object = Nodegraph.GetNodeUnderPosition(mousePosition);
		switch (button)
		{
			case 0:
				LeftClick(clicked_object);
				break;

			case 1:
				RightClick(clicked_object, mousePosition);
				break;

			default:
				break;
		}
	}

	private void DoubleClick()
	{
		if (_selectedObject == null)
		{
			return;
		}

		var windowContent = CreateInstance<EditNodeValuesWindow>();
		windowContent.Node = _selectedObject;
		var pos = _selectedObject.Pos + focusedWindow.position.position;
		windowContent.position = new Rect(pos.x, pos.y, 300, 100);
		windowContent.ShowPopup();
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
			Nodegraph.Offset(delta);
		}
	}

	private void LeftClick(Node clicked_object)
	{
		_selectedObject = clicked_object;

		_currentState = WindowState.CLICKED;
	}

	private void OnClickAddNode(Vector2 mousePosition, string text)
	{
		Changed = true;
		Nodegraph.AddNode(new Node()
		{
			Pos = mousePosition,
			Node_text = text
		});
	}

	private void OnEnable()
	{
		foreach (var nodetype in NodeTypes.Types)
		{
			if (nodetype.Menu != null)
			{
				_addNodeOptions.Add(nodetype.Menu, nodetype.Tag);
			}
		}
	}

	private void OnGUI()
	{
		if (CloseNextFrame)
		{
			GUIUtility.ExitGUI();
			return;
		}

		if (Nodegraph == null)
		{
			return;
		}
		Nodegraph.Draw(1f / _zoomAmount);
		ProcessEvents(Event.current);

		if (GUI.changed || _repaint)
		{
			_repaint = true;
			Repaint();
		}
	}

	private void ProcessEvents(Event e)
	{
		// check if the player clicks away from the window and when they click back
		UpdateMouseInWindowState(e);
		if (e.type == EventType.ScrollWheel)
		{
			_zoomAmount += e.delta.y * _zoomSpeed;
			_zoomAmount = Mathf.Clamp(_zoomAmount, 1f, _zoomMax);
		}

		switch (_currentState)
		{
			case WindowState.RIGHT_CLICKED:
			case WindowState.SELECTED:
				if (e.type == EventType.MouseDown)
				{
					Click(e.mousePosition, e.button);
					if (e.clickCount == 2)
					{
						DoubleClick();
					}
				}
				break;

			case WindowState.CLICKED:
				// escape will put us back in a starting state at any time
				bool escapeDown = (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape);
				if (e.type == EventType.MouseUp || escapeDown)
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

	private void RightClick(Node clickedObject, Vector2 mousePosition)
	{
		if (!Editable)
		{
			return;
		}

		if (_selectedObject != null && _currentState == WindowState.RIGHT_CLICKED)
		{
			Changed = true;
			Nodegraph.Connect(_selectedObject, clickedObject);
			_currentState = WindowState.SELECTED;
			_selectedObject = null;
			GUI.changed = true;
		}
		else
		{
			GenericMenu emptyClickMenu = new GenericMenu();

			if (clickedObject != null)
			{
				emptyClickMenu.AddItem(new GUIContent("Remove node"), false, () =>
				{
					Changed = true;
					_nodegraph.Delete(clickedObject);
					clickedObject = null;
				});

				emptyClickMenu.AddItem(new GUIContent("Connect"), false, () =>
				{
					Changed = true;
					_selectedObject = clickedObject;
					_currentState = WindowState.RIGHT_CLICKED;
				});

				emptyClickMenu.AddItem(new GUIContent("Disconnect"), false, () =>
				 {
					 Changed = true;
					 clickedObject.ConnectedNodes = new List<int>();
					 clickedObject = null;
				 });
			}
			else
			{
				emptyClickMenu.AddItem(new GUIContent("Generic Node"), false, () => OnClickAddNode(mousePosition, "S"));
				foreach (var menuOption in _addNodeOptions)
				{
					emptyClickMenu.AddItem(new GUIContent(menuOption.Key), false, () => OnClickAddNode(mousePosition, menuOption.Value));
				}
			}
			emptyClickMenu.ShowAsContext();
		}
	}

	private void UpdateMouseInWindowState(Event e)
	{
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
	}
}
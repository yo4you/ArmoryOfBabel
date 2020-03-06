using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


/// <summary>
/// an editor window used for creating basic nodegraphs
/// </summary>
public class NodeEditorWindow : EditorWindow
{
	private WindowState _currentState = WindowState.SELECTED;
	/// <summary>
	/// the object we last clicked
	/// </summary>
	private Node _selectedObject;
	private bool _repaint;
	private NodeGraph _nodegraph;

	public NodeGraph Nodegraph
	{
		get => _nodegraph; set
		{
			_repaint = true;
			_nodegraph = value;
		}
	}

	public bool Editable { get; internal set; } = true;
	public bool Changed { get; internal set; }
	public bool CloseNextFrame { get; internal set; }

	private enum WindowState
	{
		UNSELECTED,
		SELECTED,
		CLICKED,
		RIGHT_CLICKED
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

		Nodegraph.Draw();
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
					if (e.clickCount == 2)
					{
						DoubleClick();
					}
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

	private void DoubleClick()
	{
		if (_selectedObject == null)
		{
			return;
		}

		var windowContent = CreateInstance<RenameWindow>();
		windowContent.Node = _selectedObject;
		var pos = _selectedObject.Pos + focusedWindow.position.position;
		windowContent.position = new Rect(pos.x, pos.y, 300, 50);
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
				if (Editable)
				{
					_selectedObject = clicked_object;
				}
				_currentState = WindowState.CLICKED;
				break;
			case 1:
				if (!Editable)
				{
					break;
				}

				if (_selectedObject != null && _currentState == WindowState.RIGHT_CLICKED)
				{
					Changed = true;
					Nodegraph.Connect(_selectedObject, clicked_object);
					_currentState = WindowState.SELECTED;
					_selectedObject = null;
					GUI.changed = true;
				}
				else
				{
					GenericMenu emptyClickMenu = new GenericMenu();

					if (clicked_object != null)
					{
						emptyClickMenu.AddItem(new GUIContent("Remove node"), false, () =>
						{
							Changed = true;
							_nodegraph.Delete(clicked_object);
							clicked_object = null;
						});

						emptyClickMenu.AddItem(new GUIContent("Connect"), false, () =>
						{
							_selectedObject = clicked_object;
							_currentState = WindowState.RIGHT_CLICKED;
						});
					}
					else
					{
						emptyClickMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
					}
					emptyClickMenu.ShowAsContext();

				}
				break;
			default:
				break;
		}
	}

	private void OnClickAddNode(Vector2 mousePosition)
	{
		Changed = true;
		Nodegraph.CreateNode(new Node()
		{
			Pos = mousePosition,
			Node_text = "b"
		});
	}
}

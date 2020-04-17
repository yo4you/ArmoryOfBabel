using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponMechanicTester : MonoBehaviour
{
	public bool MovedLastFrame;

	private readonly Dictionary<string, string> _inputDefenitions = new Dictionary<string, string>
	{
		{"A","Attack 1"},
		{"B","Attack 2" },
		{"X","Special"},
	};

	private readonly Dictionary<string, string> _inputHoldDefenitions = new Dictionary<string, string>
	{
		{"AH","Attack 1"},
		{"BH","Attack 2" },
		{"XH","Special"},
	};

	private readonly Dictionary<string, NodeHandleDelegate> _nodeFunctions = new Dictionary<string, NodeHandleDelegate>
	{
		{ "NOT",    NodeBehaviour.SetState_NotNode},
		{ "AND",    NodeBehaviour.SetState_AndNode},
		{ "OR",     NodeBehaviour.SetState_OrNode},
		{ "VAL",    NodeBehaviour.SetState_ValNode},
		{ "UI",     NodeBehaviour.SetState_UINode},
		{ "OUT",    NodeBehaviour.SetState_OutNode},
		{ "MOV",    NodeBehaviour.SetState_MoveNode},
		{ "HIT",    NodeBehaviour.SetState_HitNode},
		{ "DMG",    NodeBehaviour.SetState_ValNode},
		{ "SPD",    NodeBehaviour.SetState_ValNode},
		{ "STAT",    NodeBehaviour.SetState_ValNode},
		{ "TRESH",  NodeBehaviour.SetState_TreshNode},
		{ "COPY",   NodeBehaviour.SetState_CopyNode},
		{ "DT",     NodeBehaviour.SetState_ValNode},
		{ "GENERIC",NodeBehaviour.SetState_GenericNode},
		{ "SUM",    NodeBehaviour.SetState_SumNode},
	};

	private List<Node> _callBackNodes = new List<Node>();
	private Dictionary<string, InputNode> _inputNodes = new Dictionary<string, InputNode>();

	[SerializeField] private string _mechanicGrammarName;

	private NodeGraph _mechanicGraph;

	[SerializeField]
	private float _minButtonHoldTime;

	private List<Node> _moveNodes = new List<Node>();
	private List<Node> _notNodes = new List<Node>();

	private Dictionary<Node, float> _restoreState = new Dictionary<Node, float>();
	private List<Node> _timeNodes = new List<Node>();

	private List<ChargeBarBehaviour> _uiBars = new List<ChargeBarBehaviour>();

	private Dictionary<Node, Node> _uiNodeCaps;
	private List<Node> _uiNodes = new List<Node>();

	private delegate void NodeHandleDelegate(Node prevNode, Node node, ref NodeGraph graph, bool state, float baseState);

	public float LastAttackDelay { get; set; }

	internal void CollisionCallback(Node generatingNode)
	{
		var list = new List<NodeActivationCallBack>(NodeBehaviour.Callbacks);
		var callbacknodeIndex = list.FindLastIndex(i => i.Activator == generatingNode);
		if (callbacknodeIndex != -1)
		{
			_callBackNodes.Add(list[callbacknodeIndex].Activatee);
			list.RemoveAt(callbacknodeIndex);
		}
		NodeBehaviour.Callbacks = new Stack<NodeActivationCallBack>(list);
	}

	private void AnalyseMechanicNodes()
	{
		foreach (var node in _mechanicGraph.NodeDict)
		{
			var nodeText = node.Value.Node_text;
			if (_inputDefenitions.TryGetValue(nodeText, out string keycode))
			{
				RegisterInput(keycode, node.Value);
			}
			if (_inputHoldDefenitions.TryGetValue(nodeText, out string keycode2))
			{
				RegisterHeldInput(keycode2, node.Value);
			}

			switch (nodeText)
			{
				case "NOT":
					_notNodes.Add(node.Value);
					break;

				case "DT":
					_timeNodes.Add(node.Value);
					break;

				case "UI":
					{
						_uiNodes.Add(node.Value);
						ConnectedValIntoTresh(node.Value);

						break;
					}
				case "MOV":
					_moveNodes.Add(node.Value);
					break;

				case "UIC":
					RegisterUICap(node.Value);

					break;
			}

			_restoreState.Add(node.Value, node.Value.Value);
		}
	}

	private void ConnectedValIntoTresh(Node node)
	{
		foreach (var connection in node.ConnectedNodes)
		{
			if (_mechanicGraph.NodeDict[connection].Node_text == "VAL")
			{
				_mechanicGraph.NodeDict[connection].Node_text = "TRESH";
			}
		}
	}

	private void LoadMechanicGraph()
	{
		_notNodes = new List<Node>();
		_timeNodes = new List<Node>();
		_uiNodes = new List<Node>();
		_moveNodes = new List<Node>();
		_uiNodeCaps = new Dictionary<Node, Node>();
		_inputNodes = new Dictionary<string, InputNode>();
		_restoreState = new Dictionary<Node, float>();
		// enable the right visuals
		foreach (var ui in _uiBars)
		{
			ui.gameObject.SetActive(true);
		}
		var grammars = NodeGrammar.ImportGrammars(Application.streamingAssetsPath + "/Grammar/Node/" + _mechanicGrammarName + ".json");
		// generate a simple left hand side for now
		var inputGraph = new NodeGraph();
		inputGraph.AddNode(new Node()
		{
			Node_text = "S"
		});

		_mechanicGraph = GrammarUtils.ApplyNodeGrammars("S", ref grammars, inputGraph);
		AnalyseMechanicNodes();
		// turn off extra visuals, used only when reloading mechanics
		for (int Index = _uiBars.Count - 1; Index >= _uiNodes.Count; Index--)
		{
			_uiBars[Index].gameObject.SetActive(false);
		}
	}

	private void ProccesInputNode(InputNode input, string button)
	{
		if (input.Held)
		{
			bool heldInput = Time.time - input.InstigationTime > _minButtonHoldTime;
			if (Input.GetButtonDown(button))
			{
				input.InstigationTime = Time.time;
			}
			if (Input.GetButton(button) && heldInput)
			{
				SetNodeActivity(null, input.InputHeld, true);
			}
			if (Input.GetButtonUp(button))
			{
				if (heldInput)
				{
					input.InstigationTime = float.MaxValue;
				}
				else
				{
					LastAttackDelay = Time.time - input.InstigationTime;
				}
				SetNodeActivity(null, input.Input, true);
			}
		}
		else
		{
			if (Input.GetButtonDown(button))
			{
				SetNodeActivity(null, input.Input, true);
			}
		}
	}

	private void RegisterHeldInput(string keycode, Node node)
	{
		if (_inputNodes.TryGetValue(keycode, out InputNode inputentry))
		{
			inputentry.InputHeld = node;
		}
		else
		{
			_inputNodes.Add(keycode, new InputNode() { InputHeld = node });
		}
	}

	private void RegisterInput(string keycode, Node node)
	{
		if (_inputNodes.TryGetValue(keycode, out InputNode inputentry))
		{
			inputentry.Input = node;
		}
		else
		{
			_inputNodes.Add(keycode, new InputNode() { Input = node });
		}
	}

	private void RegisterUICap(Node node)
	{
		foreach (var connectedNodeID in node.ConnectedNodes)
		{
			var connectedNode = _mechanicGraph.NodeDict[connectedNodeID];
			if (connectedNode.Node_text == "UI")
			{
				_uiNodeCaps.Add(connectedNode, node);
			}
		}
	}

	/// <summary>
	/// restore the node graph to it's "stateless" activation state
	/// </summary>
	private void RestoreNodeGraphState()
	{
		foreach (var node in _restoreState)
		{
			node.Key.Active = false;

			// only UI nodes retain their value
			if (node.Key.Node_text != "UI")
			{
				node.Key.Value = node.Value;
			}
		}
		_callBackNodes = new List<Node>();
	}

	private void SetNodeActivity(Node lastNode, Node node, bool state, List<Node> visited = default)
	{
		string node_function = node.Node_text;
		if (!_nodeFunctions.ContainsKey(node.Node_text))
		{
			node_function = "GENERIC";
		}

		if (visited == default)
		{
			visited = new List<Node>();
		}
		if (visited.Contains(node))
		{
			return;
		}
		else if (node_function != "UI")
		{
			visited.Add(node);
		}

		_nodeFunctions[node_function](lastNode, node, ref _mechanicGraph, state, _restoreState[node]);

		if (node.Active == state)
		{
			foreach (var connection in node.ConnectedNodes)
			{
				SetNodeActivity(node, _mechanicGraph.NodeDict[connection], state, visited);
			}
		}
	}

	private void Start()
	{
		foreach (var chargebar in FindObjectsOfType<ChargeBarBehaviour>())
		{
			if (chargebar.UsedForWeaponMechanics)
			{
				_uiBars.Add(chargebar);
			}
		}
		LoadMechanicGraph();

		NodeBehaviour.PlayerAttacks = FindObjectOfType<PlayerAttackControl>();
		NodeBehaviour.PlayerMovement = FindObjectOfType<PlayerMovement>();
	}

	private void Update()
	{
		// reload mechanic for debugging
		if (Input.GetKeyDown(KeyCode.F5))
		{
			LoadMechanicGraph();
		}

		UpdateNodegraphState();
		RestoreNodeGraphState();
	}

	private void UpdateInputNodeState()
	{
		if (MovedLastFrame)
		{
			MovedLastFrame = false;
			foreach (var moveNode in _moveNodes)
			{
				SetNodeActivity(null, moveNode, true);
			}
		}

		foreach (var input in _inputNodes)
		{
			// we push a new callback associated with this input
			NodeBehaviour.Callbacks.Push(new NodeActivationCallBack(null, null));
			ProccesInputNode(input.Value, input.Key);

			var callback = NodeBehaviour.Callbacks.Peek();
			// if we hit a hit response node we've new callback
			if (callback.Activatee == null || callback.Activator == null)
			{
				NodeBehaviour.Callbacks.Pop();
			}
		}
	}

	/// <summary>
	/// Update the state of the mechanic node graph
	/// </summary>
	private void UpdateNodegraphState()
	{
		// update the delta time nodes in the graph each frame
		foreach (var dtNode in _timeNodes)
		{
			dtNode.Value = Time.deltaTime;
		}

		// NOT nodes are going to be true by default so they can be flipped when activated
		foreach (var notNode in _notNodes)
		{
			notNode.Active = true;
		}

		// Resolve the callbacks we've had between last update and this
		foreach (var callbackNodes in _callBackNodes)
		{
			SetNodeActivity(null, callbackNodes, true);
		}

		// we update the UI nodes first so that the thresholds represent accurate values
		UpdateUINodeState();

		// call for all the not nodes that weren't flipped
		foreach (var notNode in _notNodes)
		{
			if (notNode.Active)
			{
				notNode.Active = false;
				SetNodeActivity(null, notNode, true);
			}
		}

		// parse trough the player input
		UpdateInputNodeState();
	}

	private void UpdateUINodeState()
	{
		for (int i = 0; i < _uiNodes.Count; i++)
		{
			Node uiNode = _uiNodes[i];
			SetNodeActivity(null, uiNode, true);
			float uicap = 100;
			if (_uiNodeCaps.TryGetValue(uiNode, out Node cap))
			{
				uicap = cap.Value;
			}

			uiNode.Value = Mathf.Clamp(uiNode.Value, 0f, uicap);
			_uiBars[i].ProgressPercentage = uiNode.Value * 100f / uicap;
		}
	}
}

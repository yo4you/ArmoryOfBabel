using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponMechanicTester : MonoBehaviour
{
	[SerializeField]
	private int _simIterations = 1000;

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

	private readonly Dictionary<string, NodeType.NodeHandleDelegate> _nodeFunctions = new Dictionary<string, NodeType.NodeHandleDelegate>
	{
		{ "GENERIC",NodeBehaviour.SetState_GenericNode},
	};

	private List<Node> _callbackNodesActivatedThisframe = new List<Node>();
	private List<Node> _healthNodes;
	private Dictionary<string, InputNode> _inputNodes = new Dictionary<string, InputNode>();

	[SerializeField]
	private string _inputString = "";

	private NodeGraph _mechanicGraph;

	[SerializeField]
	private float _minButtonHoldTime = default;

	private List<Node> _moveNodes = new List<Node>();

	[SerializeField]
	private List<string> _nodeGrammars = new List<string>();

	private List<Node> _notNodes = new List<Node>();

	[SerializeField]
	private bool _randomString = false;

	private Dictionary<Node, float> _restoreState = new Dictionary<Node, float>();

	[SerializeField]
	private int _seed = 0;

	private HudSignifierManager _signifierSystem;

	[SerializeField]
	private string _stringGrammar = default;

	private List<Node> _timeNodes = new List<Node>();
	private Dictionary<Node, Node> _uiNodeCaps;
	private List<Node> _uiNodes = new List<Node>();

	[SerializeField]
	private float _simTimeStep = 0.1f;

	private PlayerInputSimulator _playerInputSimulator;

	[SerializeField]
	private float _averageDamage = 5f;

	public float LastAttackDelay { get; set; }
	public bool MovedLastFrame { get; set; }
	public HealthComponent PlayerHealth { get; private set; }

	public void LoadMechanicGraph(int inputSeed = -1)
	{


		List<NodeGrammar> grammars = new List<NodeGrammar>();
		foreach (var grammar in _nodeGrammars)
		{
			grammars.AddRange(NodeGrammar.ImportGrammars(Application.streamingAssetsPath + "/Grammar/Node/" + grammar + ".json"));
		}

		// generate a simple left hand side for now
		var inputGraph = new NodeGraph();
		inputGraph.AddNode(new Node()
		{
			Node_text = "S"
		});

		var seed = _randomString ? UnityEngine.Random.Range(0, 1000) : _seed;
		if (inputSeed != -1)
		{
			seed = inputSeed;
		}

		var stringgrams = GrammarUtils.ImportGrammars(Application.streamingAssetsPath + "/Grammar/String/" + _stringGrammar + ".json");
		var inputString = GrammarUtils.ApplyGrammars(ref stringgrams, _inputString, seed);
		Debug.Log("mechanic generated with input string " + inputString);
		Debug.Log("Seed: " + seed);
		FindObjectOfType<SeedDisplay>()?.DisplaySeed(seed);
		_mechanicGraph = GrammarUtils.ApplyNodeGrammars(inputString, ref grammars, inputGraph, seed);
		AnalyseMechanicNodes();
		ApplySignifiers();

		AdjustBalance();
		AnalyseMechanicNodes();

		NodeBehaviour.Callbacks = new Stack<NodeActivationCallBack>();
		NodeBehaviour.PlayerAttacks = GetComponent<PlayerAttackControl>();
		NodeBehaviour.PlayerMovement = GetComponent<PlayerMovement>();
	}

	private void AdjustBalance()
	{
		PlayerAtackInputSimulator playerAttackInputSimulator = new PlayerAtackInputSimulator(this);
		NodeBehaviour.PlayerAttacks = playerAttackInputSimulator;
		PlayerMovementSimulator playerMovementSimulator = new PlayerMovementSimulator(this);
		NodeBehaviour.PlayerMovement = playerMovementSimulator;
		_playerInputSimulator = new PlayerInputSimulator(this);
		MechanicBalancer.StartAnalyze();
		for (int i = 0; i < _simIterations; i++)
		{
			playerAttackInputSimulator.Update(_simTimeStep);
			playerMovementSimulator.Update(_simTimeStep);
			_playerInputSimulator.Update(_simTimeStep);

			UpdateNodegraphState(simulate: true);
			RestoreNodeGraphState();
			foreach (var node in _uiNodes)
			{
				MechanicBalancer.RegisterUIObservation(node);
			}
		}
		MechanicBalancer.EndAnalyze(ref _mechanicGraph, _averageDamage);
	}

	internal void CollisionCallback(Node generatingNode)
	{
		var list = new List<NodeActivationCallBack>(NodeBehaviour.Callbacks);
		var callbacknodeIndex = list.FindLastIndex(i => i.Activator == generatingNode);
		if (callbacknodeIndex != -1)
		{
			_callbackNodesActivatedThisframe.Add(list[callbacknodeIndex].Activatee);
			list.RemoveAt(callbacknodeIndex);
		}
		NodeBehaviour.Callbacks = new Stack<NodeActivationCallBack>(list);
	}

	private void AnalyseMechanicNodes()
	{
		_inputNodes = new Dictionary<string, InputNode>();
		_notNodes = new List<Node>();
		_timeNodes = new List<Node>();
		_uiNodes = new List<Node>();
		_moveNodes = new List<Node>();
		_healthNodes = new List<Node>();
		_uiNodeCaps = new Dictionary<Node, Node>();
		_restoreState = new Dictionary<Node, float>();

		foreach (var node in _mechanicGraph.NodeDict)
		{
			var nodeText = node.Value.Node_text;
			if (_inputDefenitions.TryGetValue(nodeText, out string keycode))
			{
				RegisterInput(keycode, node.Value);
			}
			if (_inputHoldDefenitions.TryGetValue(nodeText, out string keycodeHeld))
			{
				RegisterHeldInput(keycodeHeld, node.Value);
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
					_uiNodes.Add(node.Value);
					ConnectedValIntoTresh(node.Value);
					break;

				case "MOV":
					_moveNodes.Add(node.Value);
					break;

				case "UIC":
					RegisterUICap(node.Value);
					break;

				case "HP":
					_healthNodes.Add(node.Value);
					node.Value.Value = PlayerHealth.HP;
					ConnectedValIntoTresh(node.Value);
					break;
			}

			_restoreState.Add(node.Value, node.Value.Value);
		}
	}

	private void ApplySignifiers()
	{
		_signifierSystem.Init(_mechanicGraph);
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
			if (node.Key.Node_text != "UI" && node.Key.Node_text != "HP")
			{
				node.Key.Value = node.Value;
			}
		}
	}

	public void SetNodeActivity(Node lastNode, Node node, bool state, List<Node> visited = default)
	{
		bool signifierNode = false;

		if (!_nodeFunctions.TryGetValue(node.Node_text, out NodeType.NodeHandleDelegate nodeFunction))
		{
			nodeFunction = NodeBehaviour.SetState_GenericNode;
			signifierNode = true;
		}
		visited = visited ?? new List<Node>();
		if (visited.Contains(node))
		{
			return;
		}
		else if (nodeFunction != NodeBehaviour.SetState_UINode)
		{
			visited.Add(node);
		}

		nodeFunction(lastNode, node, ref _mechanicGraph, state, _restoreState[node]);

		if (node.Active == state && node.Node_text != "MULT" && !signifierNode)
		{
			foreach (var connection in node.ConnectedNodes)
			{
				SetNodeActivity(node, _mechanicGraph.NodeDict[connection], state, visited);
			}
		}
	}

	private void Start()
	{
		_signifierSystem = FindObjectOfType<HudSignifierManager>();
		foreach (var nodeType in NodeTypes.Types)
		{
			_nodeFunctions.Add(nodeType.Tag, nodeType.ExecutionFunction);
		}

		PlayerHealth = GetComponent<HealthComponent>();
		LoadMechanicGraph();
	}

	private void Update()
	{
		// reload mechanic for debugging
		if (Input.GetKeyDown(KeyCode.F5))
		{
			LoadMechanicGraph();
		}

		UpdateNodegraphState();
		_signifierSystem.Step();
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
			// if we hit a hit response node we've triggered a new callback
			if (callback.Activatee == null || callback.Activator == null)
			{
				NodeBehaviour.Callbacks.Pop();
			}
		}
	}

	/// <summary>
	/// Update the state of the mechanic node graph
	/// </summary>
	private void UpdateNodegraphState(bool simulate = false)
	{
		foreach (var uiNode in _healthNodes)
		{
			uiNode.Value = PlayerHealth.HP;
		}
		// update the delta time nodes in the graph each frame
		foreach (var dtNode in _timeNodes)
		{
			dtNode.Value = simulate ? _simTimeStep : Time.deltaTime;
		}

		// NOT nodes are going to be true by default so they can be flipped when activated
		foreach (var notNode in _notNodes)
		{
			notNode.Active = true;
		}

		// Resolve the callbacks we've had between last update and this
		for (int i = _callbackNodesActivatedThisframe.Count - 1; i >= 0; i--)
		{
			Node callbackNodes = _callbackNodesActivatedThisframe[i];
			SetNodeActivity(null, callbackNodes, true);
			_callbackNodesActivatedThisframe.RemoveAt(i);
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
		if (simulate)
		{
			UpdateInputNodeStateSim();
		}
		else
		{
			UpdateInputNodeState();
		}
	}

	private void UpdateInputNodeStateSim()
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
			_playerInputSimulator.ProccesInputNode(input.Value, input.Key);

			var callback = NodeBehaviour.Callbacks.Peek();
			// if we hit a hit response node we've triggered a new callback
			if (callback.Activatee == null || callback.Activator == null)
			{
				NodeBehaviour.Callbacks.Pop();
			}
		}
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
		}
		foreach (var uiNode in _healthNodes)
		{
			SetNodeActivity(null, uiNode, true);
			PlayerHealth.HP = Mathf.Clamp(uiNode.Value, 0, PlayerHealth.StartingHP);
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponMechanicTester : MonoBehaviour
{
	private readonly Dictionary<string, KeyCode> _inputDefenitions = new Dictionary<string, KeyCode>
	{
		{"A",KeyCode.Z },
		{"B",KeyCode.X },
		{"X",KeyCode.C },
	};

	private readonly Dictionary<string, NodeHandleDelegate> _nodeFunctions = new Dictionary<string, NodeHandleDelegate>
	{
		{ "NOT",    NodeBehaviour.SetState_NotNode},
		{ "AND",    NodeBehaviour.SetState_AndNode},
		{ "OR",     NodeBehaviour.SetState_OrNode},
		{ "VAL",    NodeBehaviour.SetState_ValNode},
		{ "UI",     NodeBehaviour.SetState_UINode},
		{ "OUT",    NodeBehaviour.SetState_OutNode},
		{ "HIT",    NodeBehaviour.SetState_HitNode},
		{ "DMG",    NodeBehaviour.SetState_ValNode},
		{ "SPD",    NodeBehaviour.SetState_ValNode},
		{ "TRESH",  NodeBehaviour.SetState_TreshNode},
		{ "DT",     NodeBehaviour.SetState_ValNode},
		{ "GENERIC",NodeBehaviour.SetState_GenericNode},
	};

	private List<Node> _callBackNodes = new List<Node>();
	private Dictionary<KeyCode, Node> _inputNodes = new Dictionary<KeyCode, Node>();

	[SerializeField] private string _mechanicGrammarName;

	private NodeGraph _mechanicGraph;

	private List<Node> _notNodes = new List<Node>();

	private List<Node> _timeNodes = new List<Node>();

	private List<ChargeBarBehaviour> _uiBars = new List<ChargeBarBehaviour>();

	private Dictionary<Node, Node> _uiNodeCaps;

	private List<Node> _uiNodes = new List<Node>();

	private delegate void NodeHandleDelegate(Node prevNode, Node node, ref NodeGraph graph, bool state);

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

	private void LoadMechanicGraph()
	{
		_notNodes = new List<Node>();
		_timeNodes = new List<Node>();
		_uiNodes = new List<Node>();
		_uiNodeCaps = new Dictionary<Node, Node>();
		_inputNodes = new Dictionary<KeyCode, Node>();

		var grammars = NodeGrammar.ImportGrammars(Application.streamingAssetsPath + "/Grammar/Node/" + _mechanicGrammarName + ".json");
		var inputGraph = new NodeGraph();
		inputGraph.AddNode(new Node()
		{
			Node_text = "S"
		});

		_mechanicGraph = GrammarUtils.ApplyNodeGrammars("S", ref grammars, inputGraph);
		foreach (var node in _mechanicGraph.NodeDict)
		{
			var nodeText = node.Value.Node_text;
			if (_inputDefenitions.TryGetValue(nodeText, out KeyCode keycode))
			{
				_inputNodes.Add(keycode, node.Value);
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
						foreach (var connection in node.Value.ConnectedNodes)
						{
							if (_mechanicGraph.NodeDict[connection].Node_text == "VAL")
							{
								_mechanicGraph.NodeDict[connection].Node_text = "TRESH";
							}
						}

						break;
					}
				case "UIC":
					foreach (var connectedNodeID in node.Value.ConnectedNodes)
					{
						var connectedNode = _mechanicGraph.NodeDict[connectedNodeID];
						if (connectedNode.Node_text == "UI")
						{
							_uiNodeCaps.Add(connectedNode, node.Value);
						}
					}
					break;
			}
		}
	}

	private void SetNodeActivity(Node lastNode, Node node, bool state)
	{
		string node_function = node.Node_text;
		if (!_nodeFunctions.ContainsKey(node.Node_text))
		{
			node_function = "GENERIC";
		}
		var changed = node.Active != state;

		_nodeFunctions[node_function](lastNode, node, ref _mechanicGraph, state);

		if (node.Active == state && changed)
		{
			foreach (var connection in node.ConnectedNodes)
			{
				SetNodeActivity(node, _mechanicGraph.NodeDict[connection], state);
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
		for (int Index = _uiBars.Count - 1; Index >= _uiNodes.Count; Index--)
		{
			_uiBars[Index].gameObject.SetActive(false);
		}
		NodeBehaviour.PlayerAttacks = FindObjectOfType<PlayerAttackControl>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			LoadMechanicGraph();
		}

		foreach (var dtNode in _timeNodes)
		{
			dtNode.Value = Time.deltaTime;
		}

		foreach (var notNode in _notNodes)
		{
			notNode.Active = true;
		}

		foreach (var callbackNodes in _callBackNodes)
		{
			SetNodeActivity(null, callbackNodes, true);
		}

		for (int i = 0; i < _uiNodes.Count; i++)
		{
			Node uiNode = _uiNodes[i];
			SetNodeActivity(uiNode, uiNode, true);
			float uicap = 100;
			if (_uiNodeCaps.TryGetValue(uiNode, out Node cap))
			{
				uicap = cap.Value;
			}

			uiNode.Value = Mathf.Clamp(uiNode.Value, 0f, uicap);
			_uiBars[i].ProgressPercentage = uiNode.Value * 100f / uicap;
		}

		foreach (var notNode in _notNodes)
		{
			if (notNode.Active)
			{
				notNode.Active = false;
				SetNodeActivity(null, notNode, true);
			}
		}

		foreach (var input in _inputNodes)
		{
			NodeBehaviour.Callbacks.Push(new NodeActivationCallBack(null, null));
			if (Input.GetKeyDown(input.Key))
			{
				SetNodeActivity(null, input.Value, true);
			}
			var callback = NodeBehaviour.Callbacks.Peek();
			if (callback.Activatee == null || callback.Activator == null)
			{
				NodeBehaviour.Callbacks.Pop();
			}
		}
		foreach (var notNode in _notNodes)
		{
			SetNodeActivity(null, notNode, false);
		}

		foreach (var input in _inputNodes)
		{
			SetNodeActivity(null, input.Value, false);
		}

		foreach (var callbackNodes in _callBackNodes)
		{
			SetNodeActivity(null, callbackNodes, false);
		}

		foreach (var uiNode in _uiNodes)
		{
			SetNodeActivity(uiNode, uiNode, false);
		}
		_callBackNodes = new List<Node>();
	}
}

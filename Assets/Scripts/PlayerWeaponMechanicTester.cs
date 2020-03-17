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

	private Dictionary<KeyCode, Node> _inputNodes = new Dictionary<KeyCode, Node>();

	[SerializeField] private string _mechanicGrammarName;

	private NodeGraph _mechanicGraph;

	private Dictionary<string, NodeHandleDelegate> _nodeFunctions = new Dictionary<string, NodeHandleDelegate>
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
		{ "TRESH",    NodeBehaviour.SetState_TreshNode},
		{ "GENERIC",NodeBehaviour.SetState_GenericNode},
	};

	private List<Node> _notNodes = new List<Node>();

	private List<Node> _uiNodes = new List<Node>();

	private delegate void NodeHandleDelegate(Node prevNode, Node node, ref NodeGraph graph, bool state);

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
				SetNodeActivity(node, _mechanicGraph._nodeDict[connection], state);
			}
		}
	}

	private void Start()
	{
		var grammars = NodeGrammar.ImportGrammars(Application.streamingAssetsPath + "/Grammar/Node/" + _mechanicGrammarName + ".json");
		var inputGraph = new NodeGraph();
		inputGraph.AddNode(new Node()
		{
			Node_text = "S"
		});

		_mechanicGraph = GrammarUtils.ApplyNodeGrammars("S", ref grammars, inputGraph);
		foreach (var node in _mechanicGraph._nodeDict)
		{
			var nodeText = node.Value.Node_text;
			if (_inputDefenitions.TryGetValue(nodeText, out KeyCode keycode))
			{
				_inputNodes.Add(keycode, node.Value);
			}
			if (nodeText == "NOT")
			{
				_notNodes.Add(node.Value);
			}
			if (nodeText == "UI")
			{
				_uiNodes.Add(node.Value);
				foreach (var connection in node.Value.ConnectedNodes)
				{
					if (_mechanicGraph._nodeDict[connection].Node_text == "VAL")
					{
						_mechanicGraph._nodeDict[connection].Node_text = "TRESH";
					}
				}
			}
		}
	}

	private void Update()
	{
		foreach (var notNode in _notNodes)
		{
			notNode.Active = true;
		}

		foreach (var uiNode in _uiNodes)
		{
			SetNodeActivity(uiNode, uiNode, true);
		}
		foreach (var notNode in _notNodes)
		{
			notNode.Active = !notNode.Active;
		}

		foreach (var notNode in _notNodes)
		{
			SetNodeActivity(null, notNode, true);
		}
		foreach (var input in _inputNodes)
		{
			if (Input.GetKeyDown(input.Key))
			{
				SetNodeActivity(null, input.Value, true);
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
		foreach (var uiNode in _uiNodes)
		{
			SetNodeActivity(uiNode, uiNode, false);
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HudSignifierManager : MonoBehaviour
{
	private HudComponentIdentifier _hudData;
	private NodeGraph _mechanicGraph;

	private Dictionary<string, List<Node>> _nodesByType = new Dictionary<string, List<Node>>()
	{
		{"UI", null},
		{"UIC", null },
		{"TRESH", null },
	};

	private List<UIBarData> _uiNodeElements = new List<UIBarData>();

	internal void Init(NodeGraph mechanicGraph)
	{
		_mechanicGraph = mechanicGraph;
		foreach (var key in _nodesByType.Keys.ToList())
		{
			_nodesByType[key] = new List<Node>();
		}
		foreach (var node in mechanicGraph.NodeDict.Values)
		{
			if (_nodesByType.TryGetValue(node.Node_text, out List<Node> nodesByType))
			{
				nodesByType.Add(node);
			}
		}

		InitChargeBars();
	}

	private static void SetHudMarkingState(ChargeBarUIID hudElement, UIBarData uIBarData)
	{
		for (int markingID = 0; markingID < hudElement.Markings.Count; markingID++)
		{
			if (markingID >= uIBarData.TreshHolds.Count)
			{
				hudElement.Markings[markingID].SetActive(false);
			}
		}
	}

	private void InitChargeBars()
	{
		_uiNodeElements = new List<UIBarData>();
		foreach (var uiNode in _nodesByType["UI"])
		{
			var tresh = from id in uiNode.ConnectedNodes
						where _mechanicGraph.NodeDict[id].Node_text == "TRESH"
						select _mechanicGraph.NodeDict[id];

			var cap = from node in _nodesByType["UIC"]
					  where node.ConnectedNodes.Any(id => _mechanicGraph.NodeDict[id] == uiNode)
					  orderby node.Value ascending
					  select node;
			_uiNodeElements.Add(new UIBarData(uiNode, tresh.ToList(), cap.First()));
		}

		for (int hudID = 0; hudID < _hudData.Chargebars.Count; hudID++)
		{
			var elementExists = hudID < _uiNodeElements.Count;
			var hudElement = _hudData.Chargebars[hudID];
			hudElement.Object.SetActive(elementExists);

			if (elementExists)
			{
				var uIBarData = _uiNodeElements[hudID];
				uIBarData.HudData = hudElement;
				SetHudMarkingState(hudElement, uIBarData);
			}
		}
	}

	private void Start()
	{
		_hudData = FindObjectOfType<HudComponentIdentifier>();
	}

	private void Update()
	{
		_uiNodeElements.ForEach(UpdateChargeBars);
	}

	private void UpdateChargeBars(UIBarData chargebarData)
	{
		float value = chargebarData.Node.Value;
		float cap = chargebarData.Capacity.Value;
		chargebarData.HudData.ChargeBar.ProgressPercentage = value * 100f / cap;
		for (int i = 0; i < chargebarData.TreshHolds.Count; i++)
		{
			var treshHoldPlacementRatio = chargebarData.TreshHolds[i].Value / cap;
			GameObject marker = chargebarData.HudData.Markings[i];
			var newpos = marker.transform.position;
			newpos.x = _hudData.ChargebarsXBounds.x + (_hudData.ChargebarsXBounds.y - _hudData.ChargebarsXBounds.x) * treshHoldPlacementRatio;
			marker.transform.position = newpos;
		}
	}
}

public class UIBarData
{
	public Node Capacity;
	public ChargeBarUIID HudData;
	public Node Node;
	public List<Node> TreshHolds = new List<Node>();

	public UIBarData(Node node, List<Node> treshHolds, Node capacity)
	{
		Node = node;
		TreshHolds = treshHolds;
		Capacity = capacity;
	}
}

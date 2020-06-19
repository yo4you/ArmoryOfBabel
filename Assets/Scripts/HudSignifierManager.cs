using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HudSignifierManager : MonoBehaviour
{
	// this order must match the oder if the buffs in the hud identifier
	private string[] _buffSignals = { "fire_sign", "buff_sign", "ice_sign" };

	private string[] _buttonSignals = { "a_sign", "b_sign", "x_sign" };
	private HudComponentIdentifier _hudData;
	private NodeGraph _mechanicGraph;

	private Dictionary<string, List<Node>> _nodesByType = new Dictionary<string, List<Node>>()
	{
		{"UI", null},
		{"TYPE", null},
		{"UIC", null },
		{"TRESH", null },
		{"a_sign", null },
		{"b_sign", null },
		{"x_sign", null },
		{"buff_sign", null },
		{"fire_sign", null },
		{"ice_sign", null },
		{"toggle_sign", null },
	};

	private Func<Node>[] _skillNodeDeterminer;

	private List<UIBarData> _uiNodeElements = new List<UIBarData>();

	private enum Buttons
	{
		A, B, X
	}

	public void Step()
	{
		_uiNodeElements.ForEach(UpdateChargeBars);
		for (int skillID = 0; skillID < _skillNodeDeterminer.Length; skillID++)
		{
			var abilityHudIdentifiers = _hudData.Skills[skillID];

			var determinerFunction = _skillNodeDeterminer[skillID];
			if (determinerFunction == default)
			{
				continue;
			}
			var outNode = determinerFunction();
			if (outNode.Active)
			{
				abilityHudIdentifiers.Glow.Glow();
			}
			// this doesn't apply to the third skill
			if (skillID != 2)
			{
				var type = ReverseConnectionLookup(outNode, "TYPE").First();

				for (int iconID = 0; iconID < abilityHudIdentifiers.Icons.Count; iconID++)
				{
					abilityHudIdentifiers.Icons[iconID].SetActive(iconID == (int)type.Value);
				}

				for (int buffTypeID = 0; buffTypeID < _buffSignals.Length; buffTypeID++)
				{
					string buffSignal = _buffSignals[buffTypeID];
					var buffs = from buff in _nodesByType[buffSignal]
								where buff.ConnectedNodes.Any((int k) => _mechanicGraph.NodeDict[k] == outNode)
								select buff;
					_hudData.Skills[skillID].Statuses[buffTypeID].SetActive(buffs.FirstOrDefault() != default && buffs.First().Active);
				}
			}
		};
	}

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
		InitSkills();
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

	private IEnumerable<Node> GetConnectionWithLabel(Node uiNode, string label)
	{
		return from id in uiNode.ConnectedNodes
			   where _mechanicGraph.NodeDict[id].Node_text == label
			   select _mechanicGraph.NodeDict[id];
	}

	private void InitChargeBars()
	{
		_uiNodeElements = new List<UIBarData>();
		foreach (var uiNode in _nodesByType["UI"])
		{
			var tresh = GetConnectionWithLabel(uiNode, "TRESH");
			IOrderedEnumerable<Node> cap = ReverseConnectionLookup(uiNode, "UIC");
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

	private void InitSkills()
	{
		_skillNodeDeterminer = new Func<Node>[3];

		for (int skillIndex = 0; skillIndex < 3; skillIndex++)
		{
			foreach (var node in _nodesByType[_buttonSignals[skillIndex]])
			{
				var outnode = GetConnectionWithLabel(node, "OUT").FirstOrDefault();
				var toggleNode = GetConnectionWithLabel(node, "toggle_sign").FirstOrDefault();
				if (toggleNode != default)
				{
					var toggleSignal = GetConnectionWithLabel(toggleNode, "OUT").FirstOrDefault();
					if (toggleSignal != default)
					{
						_skillNodeDeterminer[skillIndex] = (() => toggleNode.Active ? toggleSignal : outnode);
					}
				}
				else if (outnode != default)
				{
					_skillNodeDeterminer[skillIndex] = (() => outnode);
				}
			}
			var rand = -1;
			if (_nodesByType["x_sign"].Count != 0)
			{
				_hudData.OptionalAction.SetActive(true);

				rand = UnityEngine.Random.Range(0, _hudData.Skills[2].Icons.Count);
			}
			else
			{
				_hudData.OptionalAction.SetActive(false);
			}
			for (int iconIndex = 0; iconIndex < _hudData.Skills[2].Icons.Count; iconIndex++)
			{
				_hudData.Skills[2].Icons[iconIndex].SetActive(iconIndex == rand);
			}
		}
	}

	private IOrderedEnumerable<Node> ReverseConnectionLookup(Node uiNode, string label)
	{
		return from node in _nodesByType[label]
			   where node.ConnectedNodes.Any(id => _mechanicGraph.NodeDict[id] == uiNode)
			   orderby node.Value descending
			   select node;
	}

	private void Start()
	{
		_hudData = FindObjectOfType<HudComponentIdentifier>();
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

	private class UIBarData
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
}
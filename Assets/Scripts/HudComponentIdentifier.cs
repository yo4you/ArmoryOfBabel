﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilityBaseUIID
{
	public GameObject Glow;
	public List<GameObject> Icons;
	public List<GameObject> Statuses;

	private enum Effects
	{
		Fire, Damage, Ice
	}
}

[Serializable]
public class ChargeBarUIID
{
	public ChargeBarBehaviour ChargeBar;
	public GameObject ChargeBarGlow;
	public List<GameObject> Markings;
	public GameObject Object;
}

public class HudComponentIdentifier : MonoBehaviour
{
	[SerializeField]
	private List<ChargeBarUIID> _chargebars;

	[SerializeField]
	private GameObject _optionalAction;

	[SerializeField]
	private PlayerHealthBar _playerHealth;

	[SerializeField]
	private List<AbilityBaseUIID> _skills;

	public List<ChargeBarUIID> Chargebars => _chargebars;
	public Vector2 ChargebarsXBounds { get; private set; }
	public GameObject OptionalAction => _optionalAction;
	public PlayerHealthBar PlayerHealth => _playerHealth;
	public List<AbilityBaseUIID> Skills => _skills;

	private void Start()
	{
		ChargebarsXBounds = new Vector2(_chargebars[0].Markings[0].transform.position.x, _chargebars[1].Markings[1].transform.position.x);
	}
}

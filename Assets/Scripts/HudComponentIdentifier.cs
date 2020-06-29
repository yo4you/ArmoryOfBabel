using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// set of identifiers intended to be set in the editor 
/// </summary>
[Serializable]
public class AbilityBaseUIID
{
	public GlowingMoveSignifier Glow;
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
	private List<ChargeBarUIID> _chargebars = default;

	[SerializeField]
	private GameObject _optionalAction = default;

	[SerializeField]
	private PlayerHealthBar _playerHealth = default;

	[SerializeField]
	private List<AbilityBaseUIID> _skills = default;

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
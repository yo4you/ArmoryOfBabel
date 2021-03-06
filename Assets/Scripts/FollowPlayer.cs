﻿using UnityEngine;
/// <summary>
/// lets an object follow the player, we use this to avoid some jittering and to ensure the scene doesn't have too many confusing parent child relations
/// </summary>
public class FollowPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject _player;

	private HealthComponent _playerHP;
	private Rigidbody2D _rb;
	public Vector3 Offset { get; internal set; }
	public GameObject Player { get => _player; set => _player = value; }

	private void _playerHP_OnDie()
	{
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		_playerHP.OnDie -= _playerHP_OnDie;
	}

	private void Start()
	{
		_rb = Player.GetComponent<Rigidbody2D>();
		_playerHP = Player.GetComponent<HealthComponent>();
		_playerHP.OnDie += _playerHP_OnDie;
	}

	private void Update()
	{
		transform.position = _rb.position;
	}
}
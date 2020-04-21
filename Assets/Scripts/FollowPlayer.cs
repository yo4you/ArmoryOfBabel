﻿using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject _player;

	private Rigidbody2D _rb;

	public Vector3 Offset { get; internal set; }
	public GameObject Player { get => _player; set => _player = value; }

	private void Start()
	{
		_rb = Player.GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		transform.position = _rb.position;
	}
}

﻿using UnityEngine;

public class FollowPlayerSmoothly : MonoBehaviour
{
	private BindCameraToTileMap _cameraBinder;
	private Rigidbody2D _followRb;

	[SerializeField]
	private Vector3 _offset;

	[Range(0f, 1f)]
	[SerializeField]
	private float _smoothing;

	private GameObject _toFollow;

	private void Start()
	{
		_toFollow = GameObject.FindGameObjectWithTag("Player");
		_cameraBinder = GetComponent<BindCameraToTileMap>();
		_followRb = _toFollow.GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		_cameraBinder.Target_Position = Vector3.Lerp(transform.position, (Vector3)_followRb.position + _offset, _smoothing);
	}
}

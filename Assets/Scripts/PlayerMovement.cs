﻿using UnityEngine;

/// <summary>
/// this class provides the player with the ability to move
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
	private Animator _animator;

	[SerializeField]
	[Tooltip("the maximum time the player gets after attacking to change the direction")]
	private float _attackDelay;

	private float _attackDelayTimer = 0f;
	private Vector3 _moveOffset = new Vector3();
	private Rigidbody2D _rigidBody;

	[SerializeField]
	[Tooltip("the speed at which the player moves")]
	private float _speed;

	private void FixedUpdate()
	{
		_rigidBody.MovePosition(transform.position + _speed * Time.deltaTime * _moveOffset);
		_moveOffset = new Vector3();
	}

	private void RegisterMovementToAnimator(Vector3 moveOffset)
	{
		_animator.SetFloat("x", moveOffset.x);
		_animator.SetFloat("y", moveOffset.y);
	}

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_rigidBody = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		var moveInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		// ensure that diagonal movement isn't faster than non-diagonal
		if (moveInput.magnitude > 1f)
		{
			moveInput.Normalize();
		}

		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
		{
			// we apply a short delay to the attack input so that the player can press a direction and an attack simultaniously
			if (_attackDelayTimer > 0f)
			{
				_attackDelayTimer -= Time.deltaTime;
				RegisterMovementToAnimator(moveInput);
			}
			return;
		}
		_attackDelayTimer = _attackDelay;
		RegisterMovementToAnimator(moveInput);
		_moveOffset = moveInput;
	}
}

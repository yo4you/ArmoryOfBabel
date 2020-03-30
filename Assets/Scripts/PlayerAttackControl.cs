﻿using System.Collections;
using UnityEngine;

/// <summary>
/// this class provides the player with the ability to execute certain attacks when corresponding keys are pressed
/// </summary>
public class PlayerAttackControl : MonoBehaviour
{
	private Animator _animator;
	[SerializeField] private Vector3 _offset;
	[SerializeField] private ProjectileBehaviour _projectilePrefab;
	[SerializeField] private float _projectileSpeed;
	private PlayerWeaponMechanicTester _pwmTester;
	private ReticalBehaviour _retical;
	[SerializeField] private SweepBehaviour _slashPrefab;
	[SerializeField] private SweepBehaviour _sweepPrefab;

	public bool ProccessAttackNode(float speed, float damage, int type, Node node)
	{
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
		{
			return false;
		}
		if (_retical.ActiveInput)
		{
			_animator.SetFloat("x", -Mathf.Cos(_retical.Angle * Mathf.Deg2Rad));
			_animator.SetFloat("y", -Mathf.Sin(_retical.Angle * Mathf.Deg2Rad));
		}
		_animator.speed = speed;
		switch (type)
		{
			case 0:
				// TODO power anim
				break;

			case 1:
				_animator.Play("light_hit");
				StartCoroutine(StartProjectileAttack(_projectilePrefab, speed, damage, node));
				break;

			case 2:
				_animator.Play("light_hit");
				StartCoroutine(StartMeleeAttack(_slashPrefab, speed, damage, node));
				break;

			case 3:
				_animator.Play("heavy_hit");
				StartCoroutine(StartMeleeAttack(_sweepPrefab, speed, damage, node));
				break;

			default:
				break;
		}
		return true;
	}

	public IEnumerator StartMeleeAttack(SweepBehaviour prefab, float speed, float damage, Node node)
	{
		yield return new WaitForFixedUpdate();

		var direction = new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y"));
		direction.Normalize();

		float angle = 180f + MathUtils.RoundAngleToDirection(Vector2.SignedAngle(Vector2.right, direction));
		var quat = Quaternion.Euler(0, 0, angle);
		var offset = quat * _offset;
		var hitbox = Instantiate(prefab, transform.position + offset, quat);
		hitbox.GetComponent<Animator>().speed = speed;
		hitbox.Damage = damage;
		hitbox.PWM_Tester = _pwmTester;
		hitbox.GeneratingNode = node;
	}

	public IEnumerator StartProjectileAttack(ProjectileBehaviour prefab, float speed, float damage, Node node)
	{
		yield return new WaitForFixedUpdate();

		var direction = new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y"));
		direction.Normalize();

		var projectile = Instantiate(prefab, transform.position, new Quaternion());
		projectile.MoveDir = direction * _projectileSpeed;
		projectile.Damage = damage;
		projectile.PWM_Tester = _pwmTester;
		projectile.GeneratingNode = node;
	}

	private void LateUpdate()
	{
		if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
		{
			_animator.speed = 1;
		}
	}

	private void Start()
	{
		_retical = FindObjectOfType<ReticalBehaviour>();
		_animator = GetComponent<Animator>();
		_pwmTester = GetComponent<PlayerWeaponMechanicTester>();
	}
}

using System.Collections;
using UnityEngine;

/// <summary>
/// this class provides the player with the ability to execute certain attacks when corresponding keys are pressed
/// </summary>
public class PlayerAttackControl : MonoBehaviour
{
	private Animator _animator;
	private int _attackDirToggle = 0;
	private PlayerMovement _movement;
	[SerializeField] private Vector3 _offset;
	[SerializeField] private ProjectileBehaviour _projectilePrefab;
	[SerializeField] private float _projectileSpeed;
	private PlayerWeaponMechanicTester _pwmTester;
	private ReticalBehaviour _retical;
	[SerializeField] private SweepBehaviour _slashPrefab;
	[SerializeField] private SweepBehaviour _sweepPrefab;
	public bool CanQueue { get; internal set; }

	public bool ProccessAttackNode(float speed, float damage, int type, Node node)
	{
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement") && !CanQueue)
		{
			return false;
		}
		var lastInput = new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y"));
		var direction = lastInput;

		if (_retical.ActiveInput)
		{
			direction = new Vector2(
				 -Mathf.Cos(_retical.Angle * Mathf.Deg2Rad),
				 -Mathf.Sin(_retical.Angle * Mathf.Deg2Rad)
				);
			_animator.SetFloat("x", direction.x);
			_animator.SetFloat("y", direction.y);
			if (CanQueue)
			{
				if (Vector2.Dot(direction, lastInput) > 0.5f)
				{
					_movement.ResetDashAttack();
				}
			}
		}
		else if (CanQueue)
		{
			_movement.ResetDashAttack();
		}
		_animator.speed = speed;
		switch (type)
		{
			case 0:
				// TODO power anim
				break;

			case 1:
				_animator.Play("light_hit");
				StartCoroutine(StartProjectileAttack(_projectilePrefab, speed, damage, node, direction));
				break;

			case 2:
				_animator.Play("light_hit");
				StartCoroutine(StartMeleeAttack(_slashPrefab, speed, damage, node, direction));
				break;

			case 3:
				_movement.DashAttack = Vector2.Dot(lastInput, new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y")));
				_attackDirToggle = 1 - _attackDirToggle;
				_animator.Play("heavy_hit" + _attackDirToggle);
				StartCoroutine(StartMeleeAttack(_sweepPrefab, speed, damage, node, direction));
				break;

			default:
				break;
		}
		return true;
	}

	public IEnumerator StartMeleeAttack(SweepBehaviour prefab, float speed, float damage, Node node, Vector2 direction)
	{
		yield return new WaitForFixedUpdate();
		float angle = 180f + MathUtils.RoundAngleToDirection(Vector2.SignedAngle(Vector2.right, direction));
		var quat = Quaternion.Euler(0, 0, angle);
		//var offset = quat * _offset;
		var hitbox = Instantiate(prefab, transform.position + _offset, quat);
		hitbox.GetComponent<FollowPlayer>().Player = gameObject;
		hitbox.transform.localPosition = new Vector3();
		hitbox.GetComponent<Animator>().speed = speed;
		hitbox.GetComponent<SpriteRenderer>().flipY = _attackDirToggle == 1;
		hitbox.Damage = damage;
		hitbox.PWM_Tester = _pwmTester;
		hitbox.GeneratingNode = node;
	}

	public IEnumerator StartProjectileAttack(ProjectileBehaviour prefab, float speed, float damage, Node node, Vector2 direction)
	{
		yield return new WaitForFixedUpdate();
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
		_movement = GetComponent<PlayerMovement>();
	}
}

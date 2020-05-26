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
	private StatusEffectManager _statusManager;
	[SerializeField] private SweepBehaviour _sweepPrefab;
	public bool CanQueue { get; internal set; }
	public bool Engaged { get; internal set; }

	public bool ProccessAttackNode(float speed, float damage, int type, Node node, int status = -1)
	{
		Debug.Log($"damage: {damage} status :  {status}");
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
				// a dot product of 0.5 means you're pointing the control stick in the same 90 degree quarter as your movement
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
		Engaged = true;
		switch (type)
		{
			case 0:
				// TODO power anim
				break;

			case 1:
				_animator.Play("light_hit");
				StartCoroutine(StartProjectileAttack(_projectilePrefab, speed, damage, node, direction, status));
				break;

			case 2:
				_animator.Play("light_hit" + _attackDirToggle);
				StartCoroutine(StartMeleeAttack(_slashPrefab, speed, damage, node, direction, status));
				break;

			case 3:
				_movement.DashAttack = Vector2.Dot(lastInput, new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y")));
				_animator.Play("heavy_hit" + _attackDirToggle);
				StartCoroutine(StartMeleeAttack(_sweepPrefab, speed, damage, node, direction, status));
				break;

			default:
				break;
		}
		return true;
	}

	public IEnumerator StartMeleeAttack(SweepBehaviour prefab, float speed, float damage, Node node, Vector2 direction, int status)
	{
		_attackDirToggle = 1 - _attackDirToggle;
		yield return new WaitForFixedUpdate();
		// get the amount of time the animation still has to play
		float length = _animator.GetCurrentAnimatorStateInfo(0).length - _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		float speedMult = 1f;
		// correctively speed up the attack if the player holds the button slightly too long but not long enough to trigger a held attack
		// this ensures that attacks land at a predictable interval
		if (_pwmTester.LastAttackDelay != 0f)
		{
			speedMult = length / (length - _pwmTester.LastAttackDelay);
			_pwmTester.LastAttackDelay = 0f;
		}

		_animator.speed *= speedMult;

		float angle = 180f + MathUtils.RoundAngleToDirection(Vector2.SignedAngle(Vector2.right, direction));
		var quat = Quaternion.Euler(0, 0, angle);
		var hitbox = Instantiate(prefab, transform.position, quat);
		var follow = hitbox.GetComponent<FollowPlayer>();
		follow.Player = gameObject;
		follow.Offset = _offset;
		hitbox.transform.localPosition = new Vector3();
		hitbox.GetComponent<Animator>().speed = speed * speedMult;
		hitbox.GetComponent<SpriteRenderer>().flipY = _attackDirToggle == 0;
		hitbox.Status = status;
		hitbox.Damage = damage;
		hitbox.StatusManager = _statusManager;
		hitbox.PWM_Tester = _pwmTester;
		hitbox.GeneratingNode = node;
	}

	public IEnumerator StartProjectileAttack(ProjectileBehaviour prefab, float speed, float damage, Node node, Vector2 direction, int status)
	{
		yield return new WaitForFixedUpdate();
		var projectile = Instantiate(prefab, transform.position, new Quaternion());
		projectile.Status = status;
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
			Engaged = false;
		}
	}

	private void Start()
	{
		_retical = FindObjectOfType<ReticalBehaviour>();
		_animator = GetComponent<Animator>();
		_pwmTester = GetComponent<PlayerWeaponMechanicTester>();
		_movement = GetComponent<PlayerMovement>();
		_statusManager = FindObjectOfType<StatusEffectManager>();
	}
}

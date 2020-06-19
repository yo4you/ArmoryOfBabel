using System.Collections;
using UnityEngine;

/// <summary>
/// this class provides the player with the ability to execute certain attacks when corresponding keys are pressed
/// </summary>
public class PlayerAttackControl : MonoBehaviour, IPlayerAttackControl
{
	private Animator _animator;
	private int _attackDirToggle = 0;

	private PlayerMovement _movement;
	[SerializeField] private Vector3 _offset = default;
	[SerializeField] private ProjectileBehaviour _projectilePrefab = default;

	[SerializeField] private float _projectileSpeed = default;
	private PlayerWeaponMechanicTester _pwmTester;
	private ReticalBehaviour _retical;
	[SerializeField] private SweepBehaviour _slashPrefab = default;
	private StatusEffectManager _statusManager;
	[SerializeField] private SweepBehaviour _sweepPrefab = default;
	public bool CanQueue { get; internal set; }
	public bool Engaged { get; internal set; }

	#region sounds

	[SerializeField]
	private ClipCollection _heavyHitSounds = default;

	[SerializeField]
	private ClipCollection _lightHitSounds = default;

	[SerializeField]
	private ClipCollection _projectileSounds = default;

	#endregion sounds

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
		// TODO remove this if we have a power animation
		if (type != 0)
		{
			_animator.speed = speed;
		}

		Engaged = true;
		switch (type)
		{
			case 0:
				// TODO power animation
				break;

			case 1:
				_animator.Play("light_hit" + _attackDirToggle);
				SoundManagerSingleton.Manager.PlayAudio(_projectileSounds);
				StartCoroutine(StartAttack(_projectilePrefab, speed, damage, node, direction, status));
				break;

			case 2:
				_animator.Play("light_hit" + _attackDirToggle);
				SoundManagerSingleton.Manager.PlayAudio(_lightHitSounds);
				StartCoroutine(StartAttack(_slashPrefab, speed, damage, node, direction, status));
				break;

			case 3:
				_movement.DashAttack = Vector2.Dot(lastInput, new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y")));
				_animator.Play("heavy_hit" + _attackDirToggle);
				SoundManagerSingleton.Manager.PlayAudio(_heavyHitSounds);
				StartCoroutine(StartAttack(_sweepPrefab, speed, damage, node, direction, status));
				break;

			default:
				break;
		}
		return true;
	}

	public IEnumerator StartAttack(HitBoxBehaviour prefab, float speed, float damage, Node node, Vector2 direction, int status)
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

		var hitbox = Instantiate(prefab, transform.position, new Quaternion());
		if (hitbox is ProjectileBehaviour proj)
		{
			proj.MoveDir = direction * _projectileSpeed;
		}
		else
		{
			float angle = 180f + MathUtils.RoundAngleToDirection(Vector2.SignedAngle(Vector2.right, direction));
			hitbox.transform.rotation = Quaternion.Euler(0, 0, angle);
			var follow = hitbox.GetComponent<FollowPlayer>();
			follow.Player = gameObject;
			follow.Offset = _offset;
			hitbox.transform.localPosition = new Vector3();
		}

		hitbox.GetComponent<Animator>().speed = speed * speedMult;
		hitbox.GetComponent<SpriteRenderer>().flipY = _attackDirToggle == 0;
		hitbox.Status = status;
		hitbox.Damage = damage;
		hitbox.StatusManager = _statusManager;
		hitbox.PWM_Tester = _pwmTester;
		hitbox.GeneratingNode = node;
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
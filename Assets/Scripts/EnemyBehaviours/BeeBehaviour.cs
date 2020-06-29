using System.Collections;
using UnityEngine;

/// <summary>
/// enemy behaviour, will try to maintain a specific distance to the player then freeze, charge up and fire projectiles
/// </summary>
public class BeeBehaviour : Enemy
{
	[SerializeField][Tooltip("distance between the enemy and the fired projectiles")]
	private float _bulletOffset = 1f;

	[SerializeField][Tooltip("time between standing still and firing")]
	private float _chargeUpTime = default;

	private BehaviourState _curState = BehaviourState.CHARGE;

	
	[SerializeField][Tooltip("distance between the player and enemy before the enemy runs away")]
	private float _escapeRange = default;

	[SerializeField]
	private ProjectileBehaviour _projectile = default;

	[SerializeField]
	private float _projectileSpeed = default;

	[SerializeField][Tooltip("sound efects when shooting")]
	private ClipCollection _shootClips = default;

	[SerializeField][Tooltip("time between shots")]
	private float _shootInterval = default;

	[SerializeField]
	[Tooltip("desired distance from player")]
	private float _shootRange = default;

	[SerializeField]
	[Tooltip("amount of shots to fire before regaining mobility")]
	private int _shotCount = default;

	[SerializeField]
	[Tooltip("timeframe before starting the shooting sequence")]
	private float _stallTime = default;

	[SerializeField]
	[Tooltip("cooldown after shooting")]
	private float _strikeCooldown = default;

	private enum BehaviourState
	{
		CHARGE,
		HIT,
		HITLOCK,
		RETREAT
	}

	public override void Stun()
	{
		StopAllCoroutines();
		_agent.MovementSpeed = 0;
		_curState = BehaviourState.HITLOCK;
		_sprite.color = Color.white;
	}

	public override void UnStun()
	{
		_curState = BehaviourState.RETREAT;
		_agent.MovementSpeed = _moveSpeed;
	}

	protected override void Update()
	{
		base.Update();

		Vector3 playerpos = _target.transform.position;
		var playerDistance = Vector3.Distance(playerpos, transform.position);

		switch (_curState)
		{
			case BehaviourState.CHARGE:
				if (playerDistance < _shootRange)
				{
					StartCoroutine(StartShoot());
				}
				break;

			case BehaviourState.HIT:
				if (playerDistance < _escapeRange)
				{
					StopAllCoroutines();
					_curState = BehaviourState.RETREAT;
					_agent.MovementSpeed = -_moveSpeed;
				}
				break;

			case BehaviourState.HITLOCK:
				return;

			case BehaviourState.RETREAT:
				if (playerDistance > _shootRange)
				{
					_curState = BehaviourState.CHARGE;
					_agent.MovementSpeed = _moveSpeed;
				}
				break;

			default:
				break;
		}
	}

	private void Shoot()
	{
		SoundManagerSingleton.Manager.PlayAudio(_shootClips);

		var pos = transform.position;
		var dir = (_target.transform.position - pos).normalized;
		// point projectiles at player
		var projectile = Instantiate(_projectile, pos + dir * _bulletOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir)));
		projectile.MoveDir = Vector2.right * _projectileSpeed;
	}

	private IEnumerator StartShoot()
	{
		_agent.MovementSpeed = 0;
		_curState = BehaviourState.HIT;
		yield return new WaitForSeconds(_stallTime);
		_curState = BehaviourState.HITLOCK;
		// turn red
		yield return CoroutineUtils.Interpolate(
			(time) => _sprite.color = Color.Lerp(Color.white, Color.magenta, time), _chargeUpTime);
		
		for (int i = 0; i < _shotCount; i++)
		{
			Shoot();
			yield return new WaitForSeconds(_shootInterval);
		}
		
		// regain color and reset
		yield return CoroutineUtils.Interpolate(
			(time) => _sprite.color = Color.Lerp(Color.magenta, Color.white, time), _strikeCooldown);

		_curState = BehaviourState.CHARGE;
		_agent.MovementSpeed = _moveSpeed;
	}
}
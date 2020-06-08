using System.Collections;
using UnityEngine;

public class BeeBehaviour : Enemy
{
	[SerializeField]
	private float _bulletOffset = 1f;

	[SerializeField]
	private float _chargeUpTime;

	private BehaviourState _curState = BehaviourState.CHARGE;

	[SerializeField]
	private float _escapeRange;

	[SerializeField]
	private ProjectileBehaviour _projectile;

	[SerializeField]
	private float _projectileSpeed;

	[SerializeField]
	private ClipCollection _shootClips;

	[SerializeField]
	private float _shootInterval;

	[SerializeField]
	private float _shootRange;

	[SerializeField]
	private int _shotCount;

	[SerializeField]
	private float _stallTime;

	[SerializeField]
	private float _strikeCooldown;

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
		var projectile = Instantiate(_projectile, pos + dir * _bulletOffset, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir)));
		projectile.MoveDir = Vector2.right * _projectileSpeed;
	}

	private IEnumerator StartShoot()
	{
		_agent.MovementSpeed = 0;
		_curState = BehaviourState.HIT;
		yield return new WaitForSeconds(_stallTime);
		_curState = BehaviourState.HITLOCK;
		yield return CoroutineUtils.Interpolate(
			(time) => _sprite.color = Color.Lerp(Color.white, Color.magenta, time), _chargeUpTime);
		for (int i = 0; i < _shotCount; i++)
		{
			Shoot();
			yield return new WaitForSeconds(_shootInterval);
		}
		yield return CoroutineUtils.Interpolate(
			(time) => _sprite.color = Color.Lerp(Color.magenta, Color.white, time), _strikeCooldown);

		_curState = BehaviourState.CHARGE;
		_agent.MovementSpeed = _moveSpeed;
	}
}

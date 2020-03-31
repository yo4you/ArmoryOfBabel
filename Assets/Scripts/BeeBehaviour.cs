using SAP2D;
using System.Collections;
using UnityEngine;

public class BeeBehaviour : MonoBehaviour
{
	private SAP2DAgent _agent;

	[SerializeField]
	private float _chargeUpTime;

	private BehaviourState _curState = BehaviourState.CHARGE;

	[SerializeField]
	private float _escapeRange;

	private Vector3 _lastPos = new Vector3();

	private float _moveSpeed;
	private Transform _player;

	[SerializeField]
	private ProjectileBehaviour _projectile;

	[SerializeField]
	private float _projectileSpeed;

	private RoomEvents _roomEvents;

	[SerializeField]
	private float _shootInterval;

	[SerializeField]
	private float _shootRange;

	[SerializeField]
	private int _shotCount;

	private SpriteRenderer _sprite;

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

	private void Shoot()
	{
		var pos = transform.position;
		var dir = (_player.transform.position - pos).normalized;
		var projectile = Instantiate(_projectile, pos, Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.right, dir)));
		projectile.MoveDir = Vector2.right * _projectileSpeed;
	}

	private void Start()
	{
		_roomEvents = FindObjectOfType<RoomEvents>();
		_sprite = GetComponent<SpriteRenderer>();
		_agent = GetComponent<SAP2DAgent>();
		_moveSpeed = _agent.MovementSpeed;
		_player = _agent.Target;
	}

	private IEnumerator StartShoot()
	{
		_agent.MovementSpeed = 0;
		_curState = BehaviourState.HIT;
		yield return new WaitForSeconds(_stallTime);
		_curState = BehaviourState.HITLOCK;
		{
			float inverseTime = 1f / _chargeUpTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.white, Color.magenta, time);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}

		for (int i = 0; i < _shotCount; i++)
		{
			Shoot();
			yield return new WaitForSeconds(_shootInterval);
		}
		{
			float inverseTime = 1f / _strikeCooldown;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.magenta, Color.white, time);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}

		_curState = BehaviourState.CHARGE;
		_agent.MovementSpeed = _moveSpeed;
	}

	private void Update()
	{
		var pos = transform.position;
		_sprite.flipX = _lastPos.x > pos.x;
		Vector3 playerpos = _player.transform.position;
		var playerDistance = Vector3.Distance(playerpos, pos);

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

		_lastPos = pos;
	}
}

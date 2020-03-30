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

	private Vector3 _lastPos;

	private float _moveSpeed;
	private Transform _player;

	[SerializeField]
	private ProjectileBehaviour _projectile;

	[SerializeField]
	private float _projectileSpeed;

	[SerializeField]
	private float _shootInterval;

	[SerializeField]
	private float _shootRange;

	[SerializeField]
	private int _shotCount;

	private SpriteRenderer _sprite;

	[SerializeField]
	private float _stallTime;

	private GameObject _startAnchor;

	[SerializeField]
	private float _strikeCooldown;

	private Transform _target;

	private enum BehaviourState
	{
		CHARGE,
		HIT,
		HITLOCK,
		RETREAT
	}

	private void OnDestroy()
	{
		Destroy(_startAnchor);
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
		_sprite = GetComponent<SpriteRenderer>();
		_agent = GetComponent<SAP2DAgent>();
		_startAnchor = new GameObject();
		_startAnchor.transform.position = transform.position;
		_player = _agent.Target;
		_moveSpeed = _agent.MovementSpeed;
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
		var playerDistance = Vector3.Distance(_player.transform.position, pos);

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
					_agent.Target = _startAnchor.transform;
					_agent.MovementSpeed = _moveSpeed;
				}
				break;

			case BehaviourState.HITLOCK:
				return;

			case BehaviourState.RETREAT:
				if (playerDistance > _shootRange)
				{
					_agent.Target = _player;
					_curState = BehaviourState.CHARGE;
				}

				break;

			default:
				break;
		}

		_lastPos = pos;
	}
}

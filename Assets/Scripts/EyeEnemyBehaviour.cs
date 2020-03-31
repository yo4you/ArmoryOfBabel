using SAP2D;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeEnemyBehaviour : MonoBehaviour
{
	private SAP2DAgent _agent;

	[SerializeField]
	private float _attackChargeup;

	[SerializeField]
	private float _attackDist;

	[SerializeField]
	private int _attackSequenceLength;

	[SerializeField]
	private Color _color0;

	[SerializeField]
	private Color _color1;

	private BehaviourState _curState = BehaviourState.THINKING;

	[SerializeField]
	private float _decisionInterval;

	private Vector3 _lastPos;

	private float _moveSpeed;

	[SerializeField]
	private ProjectileBehaviour _prefabColor0;

	[SerializeField]
	private ProjectileBehaviour _prefabColor1;

	[SerializeField]
	private float _projectileVelocity;

	private Rigidbody2D _rb;

	[SerializeField]
	private float _shotTimeInterval;

	private SpriteRenderer _sprite;
	private Vector2 _wanderDir;

	private enum BehaviourState
	{
		THINKING,
		WANDER,
		APPROACH,
		ATTACK
	}

	private void MakeDeciscion()
	{
		bool inRangeToAttack = Vector3.Distance(_agent.Target.transform.position, transform.position) < _attackDist;
		_curState = (BehaviourState)Random.Range(1, 3 + (inRangeToAttack ? 1 : 0));
		switch (_curState)
		{
			case BehaviourState.WANDER:
				StartCoroutine(StartWander());
				break;

			case BehaviourState.APPROACH:
				StartCoroutine(StartApproach());

				break;

			case BehaviourState.ATTACK:
				StartCoroutine(StartAttack());

				break;

			default:
				break;
		}
	}

	private void Start()
	{
		_sprite = GetComponent<SpriteRenderer>();
		_agent = GetComponent<SAP2DAgent>();
		_rb = GetComponent<Rigidbody2D>();
		_moveSpeed = _agent.MovementSpeed;
	}

	private IEnumerator StartApproach()
	{
		_agent.MovementSpeed = _moveSpeed;
		yield return new WaitForSeconds(_decisionInterval);
		_agent.MovementSpeed = 0;
		_curState = BehaviourState.THINKING;
	}

	private IEnumerator StartAttack()
	{
		_agent.MovementSpeed = 0;

		List<bool> attacks = new List<bool>();
		for (int i = 0; i < _attackSequenceLength; i++)
		{
			attacks.Add(Random.Range(0, 2) == 1);
		}

		foreach (var attack in attacks)
		{
			{
				float inverseTime = 1f / _attackChargeup;
				float time = 0;
				do
				{
					time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
					_sprite.color = Color.Lerp(Color.white, attack ? _color0 : _color1, time);
					yield return new WaitForEndOfFrame();
				} while (time != 1f);
				_sprite.color = Color.white;
			}
		}
		foreach (var attack in attacks)
		{
			float startAngle = attack ? 0f : 45f;
			_sprite.color = attack ? _color0 : _color1;
			yield return new WaitForSeconds(_shotTimeInterval);

			for (int i = 0; i < 4; i++)
			{
				var angle = startAngle + i * 90f;
				var shot = Instantiate(attack ? _prefabColor0 : _prefabColor1, transform.position, Quaternion.Euler(0, 0, angle));
				shot.MoveDir = Vector2.right * _projectileVelocity;
			}
		}

		_sprite.color = Color.white;

		yield return new WaitForSeconds(_decisionInterval);
		_curState = BehaviourState.THINKING;
	}

	private IEnumerator StartWander()
	{
		_wanderDir = Random.insideUnitCircle;

		{
			float inverseTime = 1f / _decisionInterval;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.fixedDeltaTime * inverseTime);
				_rb.MovePosition(_rb.position + _moveSpeed * _wanderDir * Time.fixedDeltaTime);
				yield return new WaitForFixedUpdate();
			} while (time != 1f);
		}
		yield return new WaitForSeconds(_decisionInterval);
		_curState = BehaviourState.THINKING;
	}

	private void Update()
	{
		var pos = transform.position;
		_sprite.flipX = _lastPos.x > pos.x;

		if (_curState == BehaviourState.THINKING)
		{
			MakeDeciscion();
		}
		_lastPos = pos;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeEnemyBehaviour : Enemy
{
	[SerializeField]
	private float _attackChargeup = default;

	[SerializeField]
	private float _attackDist = default;

	[SerializeField]
	private int _attackSequenceLength = default;

	[SerializeField]
	private Color _color0 = default;

	[SerializeField]
	private Color _color1 = default;

	private BehaviourState _curState = BehaviourState.THINKING;

	[SerializeField]
	private float _decisionInterval = default;

	[SerializeField]
	private ProjectileBehaviour _prefabColor0 = default;

	[SerializeField]
	private ProjectileBehaviour _prefabColor1 = default;

	[SerializeField]
	private float _projectileVelocity = default;

	[SerializeField]
	private ClipCollection _shotClips = default;

	[SerializeField]
	private float _shotTimeInterval = default;

	private Vector2 _wanderDir = default;

	private enum BehaviourState
	{
		THINKING,
		WANDER,
		APPROACH,
		ATTACK
	}

	public override void Stun()
	{
		StopAllCoroutines();
		_curState = BehaviourState.WANDER;
		_agent.MovementSpeed = 0;
		_sprite.color = Color.white;
	}

	public override void UnStun()
	{
		_agent.MovementSpeed = _moveSpeed;
		_curState = BehaviourState.THINKING;
	}

	protected override void Update()
	{
		base.Update();

		if (_curState == BehaviourState.THINKING)
		{
			MakeDeciscion();
		}
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
				yield return CoroutineUtils.Interpolate((time) => _sprite.color = Color.Lerp(Color.white, attack ? _color0 : _color1, time), _attackChargeup);
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
				SoundManagerSingleton.Manager.PlayAudio(_shotClips);

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

		yield return CoroutineUtils.Interpolate((time) => _rb.MovePosition(_rb.position + _moveSpeed * _wanderDir * Time.fixedDeltaTime), _decisionInterval, true);
		yield return new WaitForSeconds(_decisionInterval);
		_curState = BehaviourState.THINKING;
	}
}
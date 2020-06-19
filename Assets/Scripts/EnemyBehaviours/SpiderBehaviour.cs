using System.Collections;
using UnityEngine;

public class SpiderBehaviour : Enemy
{
	[SerializeField]
	private AudioClip _chargeSound = default;

	[SerializeField]
	private float _force = default;

	[SerializeField]
	private float _strikeCooldown = default;

	[SerializeField]
	private float _strikeTime = default;

	private bool _striking;

	[SerializeField]
	private float _strikingDistance = default;

	public IEnumerator StartStrike(Vector3 strikeDir)
	{
		_striking = true;
		_agent.MovementSpeed = 0;
		yield return CoroutineUtils.Interpolate((time) => _sprite.color = Color.Lerp(Color.white, Color.red, time), _strikeTime);
		gameObject.layer = 10;
		_rb.AddForce(strikeDir * _force);
		SoundManagerSingleton.Manager.PlayAudio(_chargeSound);
		yield return CoroutineUtils.Interpolate((time) => _sprite.color = Color.Lerp(Color.red, Color.white, time), _strikeCooldown);
		gameObject.layer = 9;
		_striking = false;
		_agent.MovementSpeed = _moveSpeed;
	}

	public override void Stun()
	{
		base.Stun();
		StopAllCoroutines();
		_rb.velocity = new Vector2();
		_agent.MovementSpeed = 0;
		_striking = false;
		_sprite.color = Color.white;
		gameObject.layer = 9;
	}

	public override void UnStun()
	{
		base.UnStun();
		_agent.MovementSpeed = _moveSpeed;
	}

	protected override void Update()
	{
		base.Update();

		if (_striking || _stunned)
		{
			return;
		}

		var targetpos = _target.transform.position;
		Vector3 position = transform.position;
		if (Vector2.Distance(position, targetpos) < _strikingDistance)
		{
			StartCoroutine(StartStrike((targetpos - position).normalized));
		}
	}
}
using System.Collections;
using UnityEngine;

public class TurtleBehaviour : Enemy
{
	private bool _attackCommited;

	[SerializeField]
	private float _blinkCount = default;

	[SerializeField]
	private float _chargeUpTime = default;

	[SerializeField]
	private GameObject _explosionPrefab = default;

	[SerializeField]
	private float _invulIndicatorTime = default;

	[SerializeField]
	private float _shakeIntensity = default;

	[SerializeField]
	private float _vulnearableTime = default;

	protected override void Start()
	{
		base.Start();

		Health.OnHit += _health_OnHit;
		Health.Invulnearable = true;
	}

	protected override void Update()
	{
		if (_attackCommited)
		{
			return;
		}

		base.Update();
	}

	private void _health_OnHit(float damage)
	{
		if (Health.Invulnearable && (!_attackCommited))
		{
			StopAllCoroutines();
			StartCoroutine(StartAggro());
		}
	}

	private IEnumerator StartAggro()
	{
		_agent.MovementSpeed = 0;
		yield return CoroutineUtils.Interpolate(
			(time) => _sprite.color = Color.Lerp(Color.gray, Color.white, time), _invulIndicatorTime);
		_attackCommited = true;
		var pos = transform.position;
		yield return CoroutineUtils.Interpolate((time) =>
		{
			_sprite.color = Color.Lerp(Color.white, Color.red, time);
			transform.position = pos + ((Vector3)Random.insideUnitCircle * _shakeIntensity);
		}, _chargeUpTime);
		transform.position = pos;
		Instantiate(_explosionPrefab, transform.position, new Quaternion());
		Health.Invulnearable = false;
		{
			float inverseTime = 1f / _vulnearableTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = (time * _blinkCount % 1f) > 0.5f ? Color.white : Color.red;

				yield return new WaitForEndOfFrame();
			} while (time != 1f || _stunned);
		}

		_sprite.color = Color.white;

		Health.Invulnearable = true;
		_agent.MovementSpeed = _moveSpeed;
		_attackCommited = false;
	}
}
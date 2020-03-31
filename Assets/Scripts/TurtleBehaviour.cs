using SAP2D;
using System.Collections;
using UnityEngine;

public class TurtleBehaviour : MonoBehaviour
{
	private SAP2DAgent _agent;
	private bool _attackCommited;

	[SerializeField]
	private float _blinkCount;

	[SerializeField]
	private float _chargeUpTime;

	[SerializeField]
	private GameObject _explosionPrefab;

	private HealthComponent _health;

	[SerializeField]
	private float _invulIndicatorTime;

	private Vector3 _lastPos;
	private float _moveSpeed;

	[SerializeField]
	private float _shakeIntensity;

	private SpriteRenderer _sprite;

	[SerializeField]
	private float _vulnearableTime;

	private void _health_OnHit(float damage)
	{
		if (_health.Invulnearable && (!_attackCommited))
		{
			StopAllCoroutines();
			StartCoroutine(StartAggro());
		}
	}

	private void Start()
	{
		_health = GetComponent<HealthComponent>();
		_agent = GetComponent<SAP2DAgent>();
		_sprite = GetComponent<SpriteRenderer>();
		_health.OnHit += _health_OnHit;
		_health.Invulnearable = true;
		_moveSpeed = _agent.MovementSpeed;
	}

	private IEnumerator StartAggro()
	{
		_agent.MovementSpeed = 0;
		{
			float inverseTime = 1f / _invulIndicatorTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.gray, Color.white, time);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}
		_attackCommited = true;
		var pos = transform.position;
		{
			float inverseTime = 1f / _chargeUpTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.white, Color.red, time);
				transform.position = pos + ((Vector3)Random.insideUnitCircle * _shakeIntensity);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}
		transform.position = pos;
		Instantiate(_explosionPrefab, transform.position, new Quaternion());
		_health.Invulnearable = false;
		{
			float inverseTime = 1f / _vulnearableTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = (time * _blinkCount % 1f) > 0.5f ? Color.white : Color.red;

				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}

		_sprite.color = Color.white;

		_health.Invulnearable = true;
		_agent.MovementSpeed = _moveSpeed;
		_attackCommited = false;
	}

	private void Update()
	{
		if (_attackCommited)
		{
			return;
		}

		var pos = transform.position;
		_sprite.flipX = _lastPos.x > pos.x;
		_lastPos = pos;
	}
}

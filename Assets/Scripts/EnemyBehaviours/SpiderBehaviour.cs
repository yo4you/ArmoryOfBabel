using SAP2D;
using System.Collections;
using UnityEngine;

public class SpiderBehaviour : MonoBehaviour, IStunnable
{
	private SAP2DAgent _agent;

	[SerializeField]
	private float _force;

	private Vector3 _lastPos;
	private float _moveSpeed;
	private Rigidbody2D _rb;
	private SpriteRenderer _sprite;

	[SerializeField]
	private float _strikeCooldown;

	[SerializeField]
	private float _strikeTime;

	private bool _striking;

	[SerializeField]
	private float _strikingDistance;

	private bool _stunned;
	private Transform _target;

	public IEnumerator StartStrike(Vector3 strikeDir)
	{
		_striking = true;
		var resetSpeed = _agent.MovementSpeed;
		_agent.MovementSpeed = 0;
		{
			float inverseTime = 1f / _strikeTime;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.white, Color.red, time);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}
		gameObject.layer = 10;

		_rb.AddForce(strikeDir * _force);
		{
			float inverseTime = 1f / _strikeCooldown;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_sprite.color = Color.Lerp(Color.red, Color.white, time);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}
		gameObject.layer = 9;
		_striking = false;
		_agent.MovementSpeed = resetSpeed;
	}

	public void Stun()
	{
		StopAllCoroutines();
		_rb.velocity = new Vector2();
		_moveSpeed = _agent.MovementSpeed;
		_agent.MovementSpeed = 0;
		_stunned = true;
		_sprite.color = Color.white;
	}

	public void UnStun()
	{
		_agent.MovementSpeed = _moveSpeed;
		_stunned = false;
	}

	private void Start()
	{
		_sprite = GetComponent<SpriteRenderer>();
		_agent = GetComponent<SAP2DAgent>();
		_rb = GetComponent<Rigidbody2D>();
		_target = _agent.Target;
	}

	private void Update()
	{
		if (_striking || _stunned)
		{
			return;
		}

		var pos = transform.position;
		_sprite.flipX = _lastPos.x > pos.x;

		var targetpos = _target.transform.position;
		if (Vector2.Distance(pos, targetpos) < _strikingDistance)
		{
			StartCoroutine(StartStrike((targetpos - transform.position).normalized));
		}

		_lastPos = pos;
	}
}

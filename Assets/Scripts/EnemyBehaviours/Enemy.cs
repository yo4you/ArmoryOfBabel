using SAP2D;
using UnityEngine;

public class Enemy : MonoBehaviour, IStunnable
{
	protected SAP2DAgent _agent;
	protected HealthComponent _health;
	protected Vector3 _lastPos;
	protected float _moveSpeed;
	protected Rigidbody2D _rb;
	protected SpriteRenderer _sprite;
	protected bool _stunned;
	protected Transform _target;

	public virtual void Stun()
	{
		_stunned = true;
	}

	public virtual void UnStun()
	{
		_stunned = false;
	}

	protected virtual void Start()
	{
		_sprite = GetComponent<SpriteRenderer>();
		_agent = GetComponent<SAP2DAgent>();
		_rb = GetComponent<Rigidbody2D>();
		_health = GetComponent<HealthComponent>();
		_target = _agent.Target;
		_moveSpeed = _agent.MovementSpeed;
	}

	protected virtual void Update()
	{
		var pos = transform.position;
		_sprite.flipX = _lastPos.x > pos.x;
		_lastPos = pos;
	}
}

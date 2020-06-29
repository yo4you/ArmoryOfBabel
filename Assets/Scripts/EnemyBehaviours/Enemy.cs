using SAP2D;
using UnityEngine;
/// <summary>
/// base class for enemies
/// </summary>
public class Enemy : MonoBehaviour, IStunnable
{
	protected SAP2DAgent _agent;
	protected float _moveSpeed;
	protected Rigidbody2D _rb;
	protected SpriteRenderer _sprite;
	protected bool _stunned;
	protected Transform _target;

	public HealthComponent Health { get; set; }

	public float MoveSpeed
	{
		get => _moveSpeed; internal set
		{
			var ratio = value / _moveSpeed;
			_moveSpeed = value;
			_agent.MovementSpeed *= ratio;
		}
	}

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
		Health = GetComponent<HealthComponent>();
		_target = _agent.Target;
		_moveSpeed = _agent.MovementSpeed;
	}

	protected virtual void Update()
	{
		var pos = transform.position;
		// turns the enemy sprite toward the player
		_sprite.flipX = pos.x > _agent.Target.position.x;
	}
}
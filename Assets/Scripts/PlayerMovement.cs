using UnityEngine;

/// <summary>
/// this class provides the player with the ability to move
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour, IPlayerMovement
{
	private Animator _animator;

	private PlayerAttackControl _attackControl;

	[SerializeField]
	[Tooltip("the maximum time the player gets after attacking to change the direction")]
	private float _attackDelay;

	[SerializeField]
	private float _dashAccelerate = default;

	private float _dashAttack;
	private float _dashAttackResetValue;
	private Vector3 _dodgeOffset = new Vector3();

	[SerializeField]
	[Tooltip("the speed at which the player dodges")]
	private float _dodgeSpeed = default;

	private Vector3 _moveOffset = new Vector3();

	[SerializeField]
	[Tooltip("the speed at which the player moves")]
	private float _moveSpeed = default;

	private PlayerWeaponMechanicTester _pwm;
	private Rigidbody2D _rigidBody;

	public float DashAttack
	{
		get => _dashAttack; internal set
		{
			_dashAttack = value;
			_dashAttackResetValue = value;
		}
	}

	public float SpeedMultiplier { get; set; } = 1f;

	public void ResetDashAttack()
	{
		_dashAttack = _dashAttackResetValue;
	}

	private void FixedUpdate()
	{
		if (_animator.GetCurrentAnimatorStateInfo(0).IsName("dodge"))
		{
			gameObject.layer = 13;
			_rigidBody.MovePosition(transform.position + _dodgeSpeed * Time.fixedDeltaTime * _dodgeOffset);
		}
		else
		{
			gameObject.layer = 8;
			_rigidBody.MovePosition(transform.position + _moveSpeed * SpeedMultiplier * Time.fixedDeltaTime * _moveOffset);
			_moveOffset = new Vector3();
		}
		if (DashAttack > 0)
		{
			_rigidBody.MovePosition(transform.position + _moveSpeed * SpeedMultiplier * Time.fixedDeltaTime * GetMovementFromAnimator() * DashAttack);
			_dashAttack -= Time.fixedDeltaTime * _dashAccelerate;
		}
	}

	private Vector3 GetMovementFromAnimator()
	{
		return new Vector3(
			_animator.GetFloat("x"),
			_animator.GetFloat("y"),
			0);
	}

	private void RegisterMovementToAnimator(Vector3 moveOffset)
	{
		_animator.SetFloat("x", moveOffset.x);
		_animator.SetFloat("y", moveOffset.y);
	}

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_rigidBody = GetComponent<Rigidbody2D>();
		_attackControl = GetComponent<PlayerAttackControl>();
		_pwm = GetComponent<PlayerWeaponMechanicTester>();
	}

	private void Update()
	{
		var moveInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		// ensure that diagonal movement isn't faster than non-diagonal
		if (moveInput.magnitude > 1f)
		{
			moveInput.Normalize();
		}

		if ((!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement")) || _attackControl.Engaged)
		{
			return;
		}
		RegisterMovementToAnimator(moveInput);

		if (Input.GetButtonDown("Dodge"))
		{
			_dodgeOffset = moveInput.normalized;

			_animator.Play("dodge");
		}
		else
		{
			_dashAttack = -1f;
			_moveOffset = moveInput;
		}
		if (_moveOffset != Vector3.zero)
		{
			_pwm.MovedLastFrame = true;
		}
	}
}
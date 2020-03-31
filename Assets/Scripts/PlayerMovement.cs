using UnityEngine;

/// <summary>
/// this class provides the player with the ability to move
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
	private Animator _animator;

	[SerializeField]
	[Tooltip("the maximum time the player gets after attacking to change the direction")]
	private float _attackDelay;

	private float _attackDelayTimer = 0f;

	private Vector3 _dodgeOffset = new Vector3();

	[SerializeField]
	[Tooltip("the speed at which the player dodges")]
	private float _dodgeSpeed;

	private Vector3 _moveOffset = new Vector3();

	[SerializeField]
	[Tooltip("the speed at which the player moves")]
	private float _moveSpeed;

	private Rigidbody2D _rigidBody;

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
			_rigidBody.MovePosition(transform.position + _moveSpeed * Time.fixedDeltaTime * _moveOffset);
			_moveOffset = new Vector3();
		}
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
	}

	private void Update()
	{
		var moveInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		// ensure that diagonal movement isn't faster than non-diagonal
		if (moveInput.magnitude > 1f)
		{
			moveInput.Normalize();
		}

		if ((!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement")) || _animator.speed != 1f)
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
			_moveOffset = moveInput;
		}
	}
}

using UnityEngine;
/// <summary>
/// this class provides the player with the ability to move
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
	[SerializeField]
	[Tooltip("the speed at which the player moves")]
	private float _speed;
	[SerializeField]
	[Tooltip("the maximum time the player gets after attacking to change the direction")]
	private float _attackDelay;
	private float _attackDelayTimer = 0f;
	private Animator _animator;
	Rigidbody2D _rigidBody;
	
	private void Start()
	{	
		_animator = GetComponent<Animator>();
		_rigidBody = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		var moveOffset = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		// ensure that diagonal movement isn't faster than non-diagonal
		if (moveOffset.magnitude > 1f)
		{
			moveOffset.Normalize();
		}

		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
		{
			// we apply a short delay to the attack input so that the player can press a direction and an attack simultaniously 
			if (_attackDelayTimer > 0f)
			{
				_attackDelayTimer -= Time.deltaTime;
				RegisterMovementToAnimator(moveOffset);
			}
			return;
		}
		_attackDelayTimer = _attackDelay;
		RegisterMovementToAnimator(moveOffset);

		_rigidBody.MovePosition(transform.position + _speed * Time.deltaTime * moveOffset);
	}

	private void RegisterMovementToAnimator(Vector3 moveOffset)
	{
		_animator.SetFloat("x", moveOffset.x);
		_animator.SetFloat("y", moveOffset.y);
	}
}

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
	[SerializeField]
	private float _speed;
	private Animator _animator;
	public bool LockMovement { get; set; }

	private void Start()
	{
		_animator = GetComponent<Animator>();
	}

	private void Update()
	{
		var moveOffset = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

		if (Mathf.Abs(moveOffset.x) < 0.07f && Mathf.Abs(moveOffset.y) < 0.07f)
		{
			return;
		}

		if (moveOffset.magnitude > 1f)
		{
			moveOffset.Normalize();
		}
		_animator.SetFloat("x", moveOffset.x);
		_animator.SetFloat("y", moveOffset.y);

		transform.Translate(_speed * Time.deltaTime * moveOffset);
	}
}

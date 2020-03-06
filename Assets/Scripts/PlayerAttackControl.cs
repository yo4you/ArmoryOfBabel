using UnityEngine;

/// <summary>
/// this class provides the player with the ability to execute certain attacks when corresponding keys are pressed
/// </summary>
public class PlayerAttackControl : MonoBehaviour
{
	private Animator _animator;

	private void Start()
	{
		_animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_animator.Play("light_hit");
		}
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			_animator.Play("heavy_hit");
		}
	}
}

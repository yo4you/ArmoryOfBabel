using System.Collections;
using UnityEngine;

/// <summary>
/// this class provides the player with the ability to execute certain attacks when corresponding keys are pressed
/// </summary>
public class PlayerAttackControl : MonoBehaviour
{
	private Animator _animator;
	[SerializeField] private Vector3 _offset;
	[SerializeField] private ProjectileBehaviour _projectilePrefab;
	[SerializeField] private GameObject _slashPrefab;
	[SerializeField] private GameObject _sweepPrefab;

	public IEnumerator StartMeleeAttack(GameObject prefab)
	{
		yield return new WaitForFixedUpdate();

		var direction = new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y"));
		direction.Normalize();

		float angle = 180f + MathUtils.RoundAngleToDirection(Vector2.SignedAngle(Vector2.right, direction));
		var quat = Quaternion.Euler(0, 0, angle);
		var offset = quat * _offset;
		Instantiate(prefab, transform.position + offset, quat);
	}

	public IEnumerator StartProjectileAttack(ProjectileBehaviour prefab)
	{
		yield return new WaitForFixedUpdate();

		var direction = new Vector2(_animator.GetFloat("x"), _animator.GetFloat("y"));
		direction.Normalize();

		Instantiate<ProjectileBehaviour>(prefab, transform.position, new Quaternion());
	}

	private void Start()
	{
		_animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement"))
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			_animator.Play("light_hit");
			//StartCoroutine(StartMeleeAttack(_slashPrefab));
			StartCoroutine(StartProjectileAttack(_projectilePrefab));
		}
		else if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			_animator.Play("heavy_hit");
			StartCoroutine(StartMeleeAttack(_sweepPrefab));
		}
	}
}

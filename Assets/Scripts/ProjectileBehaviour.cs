using UnityEngine;

public class ProjectileBehaviour : HitBoxBehaviour
{
	[SerializeField]
	private float _damage;

	public Vector3 MoveDir { get; set; }

	public void Destroy_now()
	{
		Destroy(gameObject);
	}

	private void Start()
	{
		// this is here because unity complains about serializing a field multiple times when using inheritance
		if (Damage == 0)
		{
			Damage = _damage;
		}
	}

	private void Update()
	{
		transform.Translate(MoveDir * Time.deltaTime);
	}
}

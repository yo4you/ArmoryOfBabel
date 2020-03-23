using UnityEngine;

public class ProjectileBehaviour : HitBoxBehaviour
{
	public Vector3 MoveDir { get; set; }

	private void Update()
	{
		transform.Translate(MoveDir * Time.deltaTime);
	}
}

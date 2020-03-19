using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
	public Vector3 MoveDir { get; set; }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Destroy(gameObject);
	}

	private void Update()
	{
		transform.Translate(MoveDir * Time.deltaTime);
	}
}

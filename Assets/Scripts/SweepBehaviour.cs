using UnityEngine;

public class SweepBehaviour : MonoBehaviour
{
	public void AnimationOver()
	{
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
	}
}

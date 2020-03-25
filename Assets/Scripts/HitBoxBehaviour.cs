using UnityEngine;

public abstract class HitBoxBehaviour : MonoBehaviour
{
	public float Damage { get; internal set; }
	public Node GeneratingNode { get; internal set; }
	public PlayerWeaponMechanicTester PWM_Tester { get; set; }

	public void AnimationOver()
	{
		Destroy(gameObject);
	}

	public void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.gameObject.layer != 0)
		{
			PWM_Tester.CollisionCallback(GeneratingNode);

			var health = collision.collider.gameObject.GetComponent<HealthComponent>();
			if (health != null)
			{
				health.Hit(Damage);
			}
		}
		Destroy(gameObject);
	}
}

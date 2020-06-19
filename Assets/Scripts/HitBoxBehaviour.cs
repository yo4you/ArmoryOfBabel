using UnityEngine;

public abstract class HitBoxBehaviour : MonoBehaviour
{
	[SerializeField]
	private bool _persistant = default;

	public float Damage { get; internal set; }
	public Node GeneratingNode { get; internal set; }
	public PlayerWeaponMechanicTester PWM_Tester { get; set; }
	public int Status { get; set; }
	public StatusEffectManager StatusManager { get; set; }

	public void AnimationOver()
	{
		Destroy(gameObject);
	}

	public virtual void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.gameObject.layer != 0)
		{
			if (GeneratingNode != null)
			{
				PWM_Tester.CollisionCallback(GeneratingNode);
			}
			if (Status != -1)
			{
				StatusManager?.ApplyStatus(collision.collider.gameObject, Status);
			}

			var health = collision.collider.GetComponent<HealthComponent>();
			health?.Hit(Damage);
		}
		if (!_persistant)
		{
			Destroy(gameObject);
		}
	}
}
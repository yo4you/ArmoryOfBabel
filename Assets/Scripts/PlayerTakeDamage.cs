using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour
{
	private HealthComponent _hp;

	public void OnCollisionEnter2D(Collision2D collision)
	{
	}

	private void Collide(Collision2D collision)
	{
		if (collision.collider.gameObject.layer == 10)
		{
			_hp.Hit(1);
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		Collide(collision);
	}

	private void Start()
	{
		_hp = GetComponent<HealthComponent>();
	}
}

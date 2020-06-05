using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour
{
	private HealthComponent _hp;

	[SerializeField]
	private int _invulFlashes = 4;

	private SpriteRenderer _sprite;

	private void _hp_OnHit(float damage)
	{
		StartCoroutine(CoroutineUtils.Interpolate(
				(t) => _sprite.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(t, 0.5f / _invulFlashes) * _invulFlashes * 2),
				0.4f));
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

	private void OnDestroy()
	{
		_hp.OnHit -= _hp_OnHit;
	}

	private void Start()
	{
		_hp = GetComponent<HealthComponent>();
		_hp.OnHit += _hp_OnHit;
		_sprite = GetComponent<SpriteRenderer>();
	}
}

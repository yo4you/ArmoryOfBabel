using UnityEngine;

internal class HealthComponent : MonoBehaviour
{
	[SerializeField]
	private float _startingHP;

	public float HP { get; set; }
	public bool IsPlayer { get; set; }

	internal void Hit(float damage)
	{
		HP -= damage;
		if (HP <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		if (IsPlayer)
		{
			// TODO
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void RenderUIElement()
	{
		//TODO
	}

	private void Start()
	{
		HP = _startingHP;
	}

	private void Update()
	{
		if (IsPlayer)
		{
			// TODO
		}
		else
		{
			RenderUIElement();
		}
	}
}

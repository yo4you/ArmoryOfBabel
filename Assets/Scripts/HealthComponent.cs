using UnityEngine;

public class HealthComponent : MonoBehaviour
{
	[SerializeField]
	private float _startingHP;

	private UIHealthBarManager _uiManager;
	public float HP { get; set; }
	public bool IsPlayer { get; set; }
	public float StartingHP => _startingHP;

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

	private void OnDestroy()
	{
		_uiManager?.DeallocateElement(this);
	}

	private void RenderUIElement()
	{
		//TODO
	}

	private void Start()
	{
		HP = StartingHP;
		_uiManager = FindObjectOfType<UIHealthBarManager>();
		_uiManager.AllocateElement(this);
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

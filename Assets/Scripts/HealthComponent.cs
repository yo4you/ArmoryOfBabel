using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
	private bool _invulnearable = false;

	[SerializeField]
	private bool _isPlayer;

	[SerializeField]
	private float _startingHP;

	private UIHealthBarManager _uiManager;

	public delegate void HitEvent(float damage);

	public event HitEvent OnHit;

	public float HP { get; set; }
	public bool Invulnearable { get => _invulnearable; set => _invulnearable = value; }
	public bool IsPlayer => _isPlayer;
	public float StartingHP => _startingHP;

	public void InvulnearableTimer(float time)
	{
		if (_invulnearable)
		{
			StopAllCoroutines();
		}

		StartCoroutine(StartInvulnearableTimer(time));
	}

	internal void Hit(float damage)
	{
		OnHit?.Invoke(damage);
		if (_invulnearable)
		{
			return;
		}

		HP -= damage;
		if (HP <= 0)
		{
			Die();
		}
		else
		{
			InvulnearableTimer(0.5f);
		}
	}

	private void Die()
	{
		if (IsPlayer)
		{
			// TODO player death
			gameObject.SetActive(false);
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

	private IEnumerator StartInvulnearableTimer(float time)
	{
		_invulnearable = true;
		yield return new WaitForSeconds(time);
		_invulnearable = false;
	}

	private void Update()
	{
		if (IsPlayer)
		{
			// TODO refactor player hp
		}
		else
		{
			RenderUIElement();
		}
	}
}

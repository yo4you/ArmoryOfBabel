using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
	private bool _invulnearable = false;

	private Coroutine _invulRoutine;

	[SerializeField]
	private float _invulTime = 0.1f;

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
			StopCoroutine(_invulRoutine);
		}

		_invulRoutine = StartCoroutine(StartInvulnearableTimer(time));
	}

	internal void Hit(float damage, bool dot = false)
	{
		if (!dot)
		{
			OnHit?.Invoke(damage);
			if (_invulnearable)
			{
				return;
			}
		}

		HP -= damage;
		if (HP <= 0)
		{
			Die();
		}
		else
		{
			if (!dot)
			{
				InvulnearableTimer(_invulTime);
			}
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
		if (!_isPlayer)
		{
			OnHit += (float d) => FindObjectOfType<DamageNumberController>().DisplayDamageNumber(transform.position, Invulnearable ? 0 : (int)d * 10);
		}
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

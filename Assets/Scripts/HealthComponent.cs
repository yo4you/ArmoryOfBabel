using System.Collections;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
	private bool _invulnearable = false;

	private Coroutine _invulRoutine;

	[SerializeField]
	private float _invulTime = 0.1f;

	[SerializeField]
	private bool _isPlayer = default;

	[SerializeField]
	private float _startingHP = default;

	private UIHealthBarManager _uiManager;

	public delegate void DieEvent();

	public delegate void HitEvent(float damage);

	public event DieEvent OnDie;

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
			OnHit?.Invoke(_invulnearable ? 0f : damage);

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
		OnDie?.Invoke();
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		_uiManager?.DeallocateElement(this);
	}

	private void Start()
	{
		HP = StartingHP;
		FindObjectOfType<GlobalHealthTracker>().Register(this);
		_uiManager = FindObjectOfType<UIHealthBarManager>();
		_uiManager.AllocateElement(this);
		var damagenums = FindObjectOfType<DamageNumberController>();
		if (!_isPlayer)
		{
			OnHit += (float d) => damagenums.DisplayDamageNumber(transform.position, Invulnearable ? 0 : (int)(d * 10));
		}
	}

	private IEnumerator StartInvulnearableTimer(float time)
	{
		_invulnearable = true;
		yield return new WaitForSeconds(time);
		_invulnearable = false;
	}
}
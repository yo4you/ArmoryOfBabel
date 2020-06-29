using System.Collections;
using UnityEngine;
/// <summary>
/// used for entities that need to keep track of hp and die when it's emptied
/// </summary>
public class HealthComponent : MonoBehaviour
{
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
	public bool Invulnearable { get; set; } = false;
	public bool IsPlayer => _isPlayer;
	public float StartingHP => _startingHP;

	public void InvulnearableTimer(float time)
	{
		if (Invulnearable)
		{
			StopCoroutine(_invulRoutine);
		}

		_invulRoutine = StartCoroutine(StartInvulnearableTimer(time));
	}

	internal void Hit(float damage, bool dot = false)
	{
		if (!dot)
		{
			OnHit?.Invoke(Invulnearable ? 0f : damage);

			if (Invulnearable)
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
		Invulnearable = true;
		yield return new WaitForSeconds(time);
		Invulnearable = false;
	}
}
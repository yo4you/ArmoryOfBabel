using System.Collections;
using UnityEngine;

public class WizardBehaviour : Enemy
{
	[SerializeField]
	private GameObject _aimPrefab;

	private GameObject _aimSymbol;

	[SerializeField]
	private float _aimTime;

	[SerializeField]
	private float _cooldown;

	[SerializeField]
	private float _explosionDelay;

	[SerializeField]
	private GameObject _explosionPrefab;

	[SerializeField]
	private float _flickerCount;

	[SerializeField]
	private float _idleTime;

	private Vector3 _nextTeleportPosition;

	[SerializeField]
	private float _randomTimeOffsetMax;

	[SerializeField]
	private AudioClip _warnClip;

	public void WizardLoop()
	{
		StartCoroutine(StartWizardLoop());
	}

	protected override void Start()
	{
		base.Start();
		_agent.enabled = false;
		_aimSymbol = Instantiate(_aimPrefab);
		_aimSymbol.SetActive(false);
	}

	private void OnDestroy()
	{
		if (_aimSymbol)
		{
			Destroy(_aimSymbol);
		}
	}

	private void OnDisable()
	{
		if (_aimSymbol != null)
		{
			_aimSymbol.SetActive(false);
		}

		CancelInvoke();
	}

	private void OnEnable()
	{
		float time = _idleTime + _aimTime + _explosionDelay + _cooldown + 0.5f;
		InvokeRepeating("WizardLoop", Random.Range(0, _randomTimeOffsetMax), time);
	}

	private IEnumerator StartWizardLoop()
	{
		yield return new WaitForSeconds(_idleTime);
		yield return new WaitWhile(() => _stunned);
		_nextTeleportPosition = _target.transform.position;
		_aimSymbol.SetActive(true);
		_aimSymbol.transform.position = new Vector3();
		_aimSymbol.transform.SetParent(_target.transform, false);
		yield return new WaitForSeconds(_aimTime);
		_aimSymbol.transform.parent = null;
		SoundManagerSingleton.Manager.PlayAudio(_warnClip);

		yield return CoroutineUtils.Interpolate(
			(time) => _aimSymbol.SetActive((time * _flickerCount % 1f) > 0.5f), _explosionDelay);
		_aimSymbol.SetActive(false);
		var explosion = Instantiate(_explosionPrefab, _aimSymbol.transform.position, new Quaternion());
		yield return new WaitForSeconds(_cooldown);
		yield return new WaitWhile(() => _stunned);
		transform.position = _nextTeleportPosition;
	}
}

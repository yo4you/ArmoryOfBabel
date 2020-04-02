using SAP2D;
using System.Collections;
using UnityEngine;

public class WizardBehaviour : MonoBehaviour, IStunnable
{
	private SAP2DAgent _agent;

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

	private bool _stunned;
	private Transform _target;

	public void Stun()
	{
		_stunned = true;
	}

	public void UnStun()
	{
		_stunned = false;
	}

	public void WizardLoop()
	{
		StartCoroutine(StartWizardLoop());
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

	private void Start()
	{
		_agent = GetComponent<SAP2DAgent>();
		_target = _agent.Target;
		_agent.enabled = false;
		_aimSymbol = Instantiate(_aimPrefab);
		_aimSymbol.SetActive(false);
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
		{
			float inverseTime = 1f / _explosionDelay;
			float time = 0;
			do
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
				_aimSymbol.SetActive((time * _flickerCount % 1f) > 0.5f);
				yield return new WaitForEndOfFrame();
			} while (time != 1f);
		}
		_aimSymbol.SetActive(false);
		var explosion = Instantiate(_explosionPrefab, _aimSymbol.transform.position, new Quaternion());
		yield return new WaitForSeconds(_cooldown);
		yield return new WaitWhile(() => _stunned);
		transform.position = _nextTeleportPosition;
	}
}

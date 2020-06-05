using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
	private BindCameraToTileMap _binder;

	[SerializeField]
	private float _playerShakeIntensity;

	[SerializeField]
	private int _playershakes;

	[SerializeField]
	private float _playerShakeTime;

	public void Shake(float time, float intensity, int shakes)
	{
		StartCoroutine(StartShake());
		IEnumerator StartShake()
		{
			float singleShakeTime = time / shakes;
			for (int i = 0; i < shakes; i++)
			{
				yield return new WaitForSeconds(singleShakeTime);
				_binder.ShakeOffset = (Vector3)Random.insideUnitCircle * intensity;
			}
			_binder.ShakeOffset = new Vector3();
		}
	}

	private void ScreenShake_OnHit(float damage)
	{
		Shake(_playerShakeTime, _playerShakeIntensity, _playershakes);
	}

	private void Start()
	{
		_binder = GetComponent<BindCameraToTileMap>();
		FindObjectOfType<PlayerAttackControl>().gameObject.GetComponent<HealthComponent>().OnHit += ScreenShake_OnHit;
	}
}

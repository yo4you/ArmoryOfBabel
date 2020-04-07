using UnityEngine;

public class ChargeBarBehaviour : MonoBehaviour
{
	private RectTransform _bar;

	private float _progressPercentage;

	[SerializeField]
	private bool _usedForWeaponMechanics;

	public float ProgressPercentage
	{
		get => _progressPercentage; set
		{
			_bar = _bar ?? GetProgressElement();
			_progressPercentage = Mathf.Clamp(value, 0f, 100f);
			var newpos = _bar.localPosition;
			newpos.x = -_bar.rect.width * (100f - _progressPercentage) / 100f;
			if (!float.IsNaN(newpos.x))
			{
				_bar.localPosition = newpos;
			}
		}
	}

	public bool UsedForWeaponMechanics => _usedForWeaponMechanics;

	private RectTransform GetProgressElement()
	{
		return gameObject.transform.Find("mask").Find("progress") as RectTransform;
	}

	private void Start()
	{
		_bar = GetProgressElement();
		ProgressPercentage = 0f;
	}
}

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
			_progressPercentage = Mathf.Clamp(value, 0f, 100f);
			var newpos = _bar.localPosition;
			newpos.x = -_bar.rect.width * (100f - _progressPercentage) / 100f;
			_bar.localPosition = newpos;
		}
	}

	public bool UsedForWeaponMechanics => _usedForWeaponMechanics;

	private void Start()
	{
		_bar = gameObject.transform.Find("mask").Find("progress") as RectTransform;
		ProgressPercentage = 0f;
	}
}

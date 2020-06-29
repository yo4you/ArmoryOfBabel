using UnityEngine;

public class ChargeBarBehaviour : MonoBehaviour
{
	[SerializeField]
	[Tooltip("if the bar should be vertical or horizontal")]
	private Axis _axis = Axis.X;

	private RectTransform _bar;
	private RectTransform _mask;

	private float _progressPercentage;

	[SerializeField]
	private bool _usedForWeaponMechanics = default;

	private enum Axis
	{
		X, Y
	}

	public float ProgressPercentage
	{
		get => _progressPercentage; set
		{
			if (_bar == null)
			{
				GetProgressElement();
			}
			// we move the mask and the bar in opposite directions, this is done because unity requires the mask to parent the sprite
			_progressPercentage = Mathf.Clamp(value, 0f, 100f);
			var newposMask = _mask.localPosition;
			newposMask[(int)_axis] = -_bar.rect.width * (100f - _progressPercentage) / 100f;
			if (!float.IsNaN(newposMask[(int)_axis]))
			{
				_mask.localPosition = newposMask;
			}
			// we want to move the mask to indicate progress but not the child
			var newposbar = _bar.localPosition;
			newposbar[(int)_axis] = _bar.rect.width * (100f - _progressPercentage) / 100f;
			_bar.localPosition = newposbar;
		}
	}

	public bool UsedForWeaponMechanics => _usedForWeaponMechanics;

	private void GetProgressElement()
	{
		_mask = gameObject.transform.Find("mask") as RectTransform;
		_bar = _mask.Find("progress") as RectTransform;
	}

	private void Start()
	{
		GetProgressElement();
		ProgressPercentage = 0f;
	}
}
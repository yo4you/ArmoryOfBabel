using UnityEngine;
using UnityEngine.UI;

public class GlowingMoveSignifier : MonoBehaviour
{
	[SerializeField]
	private float _fadeTime = 0.2f;

	private Image _image;
	private LockState _state;
	private float _timer;

	public enum LockState
	{
		UNLOCKED,
		TRUE,
		FAlSE,
	}

	public LockState State
	{
		get => _state; set
		{
			_state = value;
			switch (value)
			{
				case LockState.UNLOCKED:
					break;

				case LockState.TRUE:
					SetAplha(1f);
					break;

				case LockState.FAlSE:
					_timer = 1f;
					break;

				default:
					break;
			}
		}
	}

	public void Glow()
	{
		if (_state != LockState.FAlSE)
		{
			_timer = 1f;
		}
	}

	private void SetAplha(float a)
	{
		var color = _image.color;
		color.a = Mathf.Clamp01(a);
		_image.color = color;
	}

	private void Start()
	{
		_image = GetComponent<Image>();
	}

	private void Update()
	{
		if (_state != LockState.TRUE)
		{
			_timer -= Time.deltaTime / _fadeTime;
		}
		SetAplha(_timer);
	}
}
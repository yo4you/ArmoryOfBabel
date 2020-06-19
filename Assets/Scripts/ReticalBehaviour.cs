using UnityEngine;

public class ReticalBehaviour : MonoBehaviour
{
	private bool _activeInput;
	private float _angle;
	private Camera _camera;
	private float _deadZone = 0.3f;
	private SpriteMask _mask;

	[SerializeField]
	private float _smoothing = default;

	public bool ActiveInput => _activeInput = true;
	public float Angle => _angle;

	private void Start()
	{
		_mask = GetComponentInChildren<SpriteMask>();
		_camera = Camera.main;
	}

	private void Update()
	{
		if (Input.GetJoystickNames().Length == 0)
		{
			var mousepos = Input.mousePosition;
			var worldpos = _camera.ScreenToWorldPoint(mousepos);
			_angle = Vector2.SignedAngle(Vector2.right, transform.position - worldpos);
		}
		else
		{
			var input = new Vector2(-Input.GetAxis("AimHorizontal"), Input.GetAxis("AimVertical"));
			_activeInput = input.magnitude > _deadZone;
			_mask.gameObject.SetActive(_activeInput);
			_angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
		}
		_mask.transform.rotation = Quaternion.Slerp(_mask.transform.rotation, Quaternion.Euler(0, 0, _angle + 90f), _smoothing * Time.deltaTime);
	}
}
using UnityEngine;

public class FollowPlayerSmoothly : MonoBehaviour
{
	private BindCameraToTileMap _cameraBinder;

	[SerializeField]
	private Vector3 _offset;

	[Range(0f, 1f)]
	[SerializeField]
	private float _smoothing;

	private GameObject _toFollow;

	private void Start()
	{
		_toFollow = GameObject.FindGameObjectWithTag("Player");
		_cameraBinder = GetComponent<BindCameraToTileMap>();
	}

	private void Update()
	{
		_cameraBinder.Target_Position = Vector3.Lerp(transform.position, _toFollow.transform.position + _offset, _smoothing);
	}
}

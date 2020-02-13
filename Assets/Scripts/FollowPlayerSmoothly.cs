using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerSmoothly : MonoBehaviour
{
	[Range(0f,1f)]
	[SerializeField]
	float _smoothing;
	[SerializeField]
	Vector3 _offset;
	GameObject _toFollow;
	private BindCameraToTileMap _cameraBinder;

	void Start()
	{
		_toFollow = GameObject.FindGameObjectWithTag("Player");
		_cameraBinder = GetComponent<BindCameraToTileMap>();
	}

	void Update()
    {
		_cameraBinder.Target = Vector3.Lerp(transform.position, _toFollow.transform.position + _offset, _smoothing);
    }
}

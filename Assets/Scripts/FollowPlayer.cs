using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject _player;

	private void Update()
	{
		transform.position = _player.transform.position;
	}
}

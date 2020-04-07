using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
	[SerializeField]
	private GameObject _player;

	public Vector3 Offset { get; internal set; }
	public GameObject Player { get => _player; set => _player = value; }

	private void Update()
	{
		transform.position = Player.transform.position + (transform.rotation * Offset);
	}
}

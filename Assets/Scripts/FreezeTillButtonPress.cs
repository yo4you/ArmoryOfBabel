using UnityEngine;

public class FreezeTillButtonPress : MonoBehaviour
{
	private void Start()
	{
		Time.timeScale = 0.0001f;
	}

	private void Update()
	{
		if (Input.anyKeyDown)
		{
			Time.timeScale = 1;
			Destroy(gameObject);
		}
	}
}
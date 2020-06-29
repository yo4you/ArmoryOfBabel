using UnityEngine;
/// <summary>
/// freezes the game till a button has been pressed, intended to be used at the start of the game to display instructions first
/// </summary>
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
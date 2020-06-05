using UnityEngine;
using UnityEngine.UI;

public class SeedDisplay : MonoBehaviour
{
	private Text _text;

	public void DisplaySeed(int seed)
	{
		_text.text = "SEED : " + seed.ToString();
	}

	private void Start()
	{
		_text = GetComponent<Text>();
	}
}

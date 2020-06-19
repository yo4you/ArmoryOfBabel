using UnityEngine;
using UnityEngine.UI;

public class EnemyCounterUI : MonoBehaviour
{
	private Text _prefix;
	private Text _tally;

	public void SetTally(int count, int max)
	{
		_tally.text = $" {count} / {max}";
	}

	public void SetVisible(bool visible)
	{
		_tally.enabled = visible;
		_prefix.enabled = visible;
	}

	private void Start()
	{
		_tally = transform.GetChild(0).GetComponent<Text>();
		_prefix = GetComponent<Text>();
	}
}
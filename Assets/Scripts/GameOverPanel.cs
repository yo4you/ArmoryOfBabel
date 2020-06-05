using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
	[SerializeField]
	private float _delay = 0.5f;

	[SerializeField]
	private GameObject _display;

	private bool _inputEnabled;

	private void GameOverPanel_OnDie()
	{
		_display.SetActive(true);
		StartCoroutine(StartAnyButtonDelay());

		IEnumerator StartAnyButtonDelay()
		{
			yield return new WaitForSeconds(_delay);
			_inputEnabled = true;
		}
	}

	private void Start()
	{
		FindObjectOfType<PlayerAttackControl>().gameObject.GetComponent<HealthComponent>().OnDie += GameOverPanel_OnDie;
	}

	private void Update()
	{
		if (_inputEnabled && Input.anyKey)
		{
			string currentSceneName = SceneManager.GetActiveScene().name;
			SceneManager.LoadScene(currentSceneName);
		}
	}
}

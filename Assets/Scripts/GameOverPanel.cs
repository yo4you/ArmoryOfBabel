using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
	[SerializeField]
	private float _delay = .3f;

	[SerializeField]
	private GameObject _display = default;

	[SerializeField]
	private float _inputDelay = 0.5f;

	private bool _inputEnabled;

	private void GameOverPanel_OnDie()
	{
		StartCoroutine(StartAnyButtonDelay());

		IEnumerator StartAnyButtonDelay()
		{
			yield return new WaitForSeconds(_delay);
			_display.SetActive(true);
			yield return new WaitForSeconds(_inputDelay);
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
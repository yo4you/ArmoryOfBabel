using UnityEngine;
using UnityEngine.UI;

public class SeedInsertUI : MonoBehaviour
{
	private InputField _field;
	private Text _text;

	public void Submit(string input)
	{
		Time.timeScale = 1;
		if (int.TryParse(input, out int result))
		{
			FindObjectOfType<PlayerWeaponMechanicTester>().LoadMechanicGraph(result);
		}
		_text.enabled = false;
	}

	private void Start()
	{
		_field = GetComponent<InputField>();
		_text = _field.textComponent;
		_field.enabled = true;
		_text.enabled = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			_field.enabled = true;
			Time.timeScale = 0;
			_field.ActivateInputField();
			_text.enabled = true;
		}
	}
}
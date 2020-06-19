using UnityEngine;
using UnityEngine.UI;

public class DamageNumberController : MonoBehaviour
{
	private Animator[] _animators;
	private string _animName = "fall";
	private Camera _camera;
	private int _lastPopped = 0;

	public void DisplayDamageNumber(Vector3 worldPos, int num)
	{
		_lastPopped = (_lastPopped + 1) % _animators.Length;
		_animators[_lastPopped].transform.position = _camera.WorldToScreenPoint(worldPos);
		_animators[_lastPopped].GetComponent<Text>().text = num.ToString();
		_animators[_lastPopped].Play(_animName);
	}

	private void Start()
	{
		_animators = GetComponentsInChildren<Animator>();
		_camera = Camera.main;
	}
}
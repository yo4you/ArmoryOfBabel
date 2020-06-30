using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// class used to help control the damage numbers that fly off to indicate the amount of damage the player is inflicting
/// </summary>
public class DamageNumberController : MonoBehaviour
{
	private Animator[] _animators;
	private string _animName = "fall";
	private Camera _camera;
	private int _lastPopped = 0;

	public void DisplayDamageNumber(Vector3 worldPos, int num)
	{
		if (num == 0) 
			return;
		_lastPopped = (_lastPopped + 1) % _animators.Length;
		_animators[_lastPopped].transform.position = _camera.WorldToScreenPoint(worldPos);
		_animators[_lastPopped].GetComponent<Text>().text = num.ToString();
		_animators[_lastPopped].Play(_animName);
	}

	private void Start()
	{
		// we pool these damage numbers as children
		_animators = GetComponentsInChildren<Animator>();
		_camera = Camera.main;
	}
}
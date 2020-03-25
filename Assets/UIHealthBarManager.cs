using System.Collections.Generic;
using UnityEngine;

public class UIHealthBarManager : MonoBehaviour
{
	private Camera _camera;
	private HashSet<HealthComponent> _elements = new HashSet<HealthComponent>();
	private List<ChargeBarBehaviour> _healthBarPool = new List<ChargeBarBehaviour>();

	[SerializeField]
	private Vector3 _offset;

	public void AllocateElement(HealthComponent healthComponent)
	{
		_elements.Add(healthComponent);
	}

	public void DeallocateElement(HealthComponent healthComponent)
	{
		_elements.Remove(healthComponent);
	}

	private void FixedUpdate()
	{
		int uiBarIndex = 0;
		foreach (var element in _elements)
		{
			if (element.gameObject.activeSelf)
			{
				var screenpos = _camera.WorldToScreenPoint(element.transform.position);
				if (!_camera.pixelRect.Contains(screenpos))
				{
					continue;
				}
				var hpbar = _healthBarPool[uiBarIndex];
				hpbar.transform.position = screenpos + _offset;
				hpbar.gameObject.SetActive(true);
				hpbar.ProgressPercentage = (element.HP / element.StartingHP) * 100f;
				uiBarIndex++;
			}
			if (uiBarIndex >= _healthBarPool.Count)
			{
				break;
			}
		}
		for (int i = uiBarIndex; i < _healthBarPool.Count; i++)
		{
			_healthBarPool[i].gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		_healthBarPool.AddRange(GetComponentsInChildren<ChargeBarBehaviour>());
		foreach (var item in _healthBarPool)
		{
			item.gameObject.SetActive(false);
		}
		_camera = Camera.main;

		_offset.x *= _camera.pixelRect.width;
		_offset.y *= _camera.pixelRect.height;
	}
}

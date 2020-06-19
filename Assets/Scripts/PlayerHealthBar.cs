using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
	private ChargeBarBehaviour _bar;
	private HealthComponent _hp;

	private void Start()
	{
		foreach (var hp in FindObjectsOfType<HealthComponent>())
		{
			if (hp.IsPlayer)
			{
				_hp = hp;
			}
		}
		_bar = GetComponent<ChargeBarBehaviour>();
	}

	private void Update()
	{
		_bar.ProgressPercentage = (_hp.HP / _hp.StartingHP) * 100f;
	}
}
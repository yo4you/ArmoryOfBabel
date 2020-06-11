using System.Collections;
using UnityEngine;

/// <summary>
/// Status effect that applies a damage over time effect
/// </summary>
public class BurnStatus : StatusEffect
{
	[SerializeField]
	private float _burnAmount = 1f;

	[SerializeField]
	private int _burnTicks = 3;

	[SerializeField]
	private float _burnTickTime = 1f;

	public override void Apply(Enemy enemy)
	{
		if (enemy != null)
		{
			_active = true;
		}
		StartCoroutine(StartBurn(enemy));
	}

	private IEnumerator StartBurn(Enemy enemy)
	{
		yield return null;
		for (int i = 0; i < _burnTicks; i++)
		{
			if (!enemy)
			{
				break;
			}
			enemy.Health.Hit(_burnAmount, true);
			yield return new WaitForSeconds(_burnTickTime);
		}
		Destroy(this);
	}
}

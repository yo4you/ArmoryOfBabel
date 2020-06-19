using System.Collections;
using UnityEngine;

public class IceStatus : StatusEffect
{
	[SerializeField]
	private float _freezeTime = 10f;

	public override void Apply(Enemy enemy)
	{
		if (enemy != null)
		{
			_active = true;
		}
		StartCoroutine(StartFreeze(enemy));
	}

	private IEnumerator StartFreeze(Enemy enemy)
	{
		if (enemy)
		{
			enemy.MoveSpeed = 0.1f;
			yield return new WaitForSeconds(_freezeTime);
			enemy.MoveSpeed = 1f;
		}

		Destroy(this);
	}
}
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
	[SerializeField]
	private List<StatusEffect> _statuses = new List<StatusEffect>();

	public void ApplyStatus(GameObject target, int ID)
	{
		StatusEffect container = _statuses[ID];
		var statusEffect = target.AddComponent(container.GetType()) as StatusEffect;
		var visual = Instantiate(container.Visual, new Vector3(), new Quaternion(), target.transform);
		visual.transform.localPosition = new Vector3();
		statusEffect.Visual = visual;
		statusEffect.Apply(target.GetComponent<Enemy>());
	}
}

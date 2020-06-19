using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class GlobalHealthTracker : MonoBehaviour
{
	public delegate void RegisterHealthEvent(HealthComponent healthComponent);

	public event RegisterHealthEvent OnRegisterHealth;

	internal void Register(HealthComponent healthComponent)
	{
		OnRegisterHealth?.Invoke(healthComponent);
	}
}
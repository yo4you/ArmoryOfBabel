using UnityEngine;
/// <summary>
/// a system for tracking when a new health element comes into the scene 
/// </summary>
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
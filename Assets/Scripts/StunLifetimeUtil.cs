using System.Collections;
using UnityEngine;

/// <summary>
/// this class exists to ensure the "stun" coroutines are bound to an object with a longer lifetime than the stunned entity
/// </summary>
public class StunLifetimeUtil : MonoBehaviour
{
	internal void StartStun(IStunnable stunnable, float stunTime)
	{
		StartCoroutine(StartUnstunDelay(stunnable, stunTime));
	}

	private IEnumerator StartUnstunDelay(IStunnable stunnable, float stunTime)
	{
		yield return new WaitForSeconds(stunTime);
		stunnable?.UnStun();
	}
}
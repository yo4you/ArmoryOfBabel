using System.Collections;
using UnityEngine;

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

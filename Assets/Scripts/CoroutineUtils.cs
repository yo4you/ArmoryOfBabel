using System.Collections;
using UnityEngine;
/// <summary>
/// utility class for some coroutine functions often used
/// </summary>
public static class CoroutineUtils
{
	public delegate void InterpolatableAction(float t);

	public static IEnumerator Interpolate(InterpolatableAction action, float totalTime, bool fixedTime = false)
	{
		float inverseTime = 1f / totalTime;
		float time = 0;
		do
		{
			if (fixedTime)
			{
				time = Mathf.Clamp01(time + Time.fixedDeltaTime * inverseTime);
			}
			else
			{
				time = Mathf.Clamp01(time + Time.deltaTime * inverseTime);
			}
			action(time);
			if (fixedTime)
			{
				yield return new WaitForFixedUpdate();
			}
			else
			{
				yield return null;
			}
		} while (time != 1f);
	}
}
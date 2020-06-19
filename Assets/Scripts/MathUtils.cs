using UnityEngine;

/// <summary>
/// this class is for privind missing math utitilies
/// </summary>
internal static class MathUtils
{
	/// <summary>
	/// provides the maximum x y and z values for all provided vectors
	/// </summary>
	/// <param name="vectors"></param>
	/// <returns></returns>
	public static Vector3 MaxBound(Vector3[] vectors)
	{
		var outp = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		// iterate over dimensions
		for (int i = 0; i < 3; i++)
		{
			foreach (var vec in vectors)
			{
				outp[i] = Mathf.Max(outp[i], vec[i]);
			}
		}
		return outp;
	}

	/// <summary>
	/// calculates the minimum x y and z values for all provided vectors
	/// </summary>
	/// <param name="vectors"></param>
	/// <returns></returns>
	public static Vector3 MinBound(Vector3[] vectors)
	{
		var outp = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		// iterate over dimentions
		for (int i = 0; i < 3; i++)
		{
			foreach (var vec in vectors)
			{
				outp[i] = Mathf.Min(outp[i], vec[i]);
			}
		}
		return outp;
	}

	public static float RoundAngleToDirection(float angle)
	{
		while (angle < 0f)
		{
			angle += 360f;
		}
		return ((int)(((angle + 22.5f) % 360f) / 45f)) * 45f;
	}
}
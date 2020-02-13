using UnityEngine;

static class MathUtils
{
	public static Vector3 MinBound(Vector3[] vectors)
	{
		var outp = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		for (int i = 0; i < 3; i++)
		{
			foreach (var vec in vectors)
			{
				outp[i] = Mathf.Min(outp[i], vec[i]);
			}
		}
		return outp;
	}

	public static Vector3 MaxBound(Vector3[] vectors)
	{
		var outp = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 3; i++)
		{
			foreach (var vec in vectors)
			{
				outp[i] = Mathf.Max(outp[i], vec[i]);
			}
		}
		return outp;
	}
}
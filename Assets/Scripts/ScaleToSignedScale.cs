using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// this class ensures the scale of the object stays as a whole integer so we can use blendtrees for spriteanimation
/// </summary>
public class ScaleToSignedScale : MonoBehaviour
{
    void LateUpdate()
    {
		transform.localScale = new Vector3(
			Mathf.Sign(transform.localScale.x),
			Mathf.Sign(transform.localScale.y),
			Mathf.Sign(transform.localScale.z)
			);

	}
}

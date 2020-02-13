using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackControl : MonoBehaviour
{
	private Animator _animator;

	private void Start()
	{
		_animator = GetComponent<Animator>();
	}
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_animator.Play("light_hit");
		}
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			_animator.Play("heavy_hit");

		}
	}

}

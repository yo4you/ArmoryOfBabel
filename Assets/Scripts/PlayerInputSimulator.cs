using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

internal class PlayerInputSimulator : IInputSim
{
	private Dictionary<string, IEnumerator<float>> _buttonStates = new Dictionary<string, IEnumerator<float>>();
	private float _timeStep;

	private PlayerWeaponMechanicTester _playerWeaponMechanicTester;
	private float _period = 25f;

	public PlayerInputSimulator(PlayerWeaponMechanicTester playerWeaponMechanicTester)
	{
		_playerWeaponMechanicTester = playerWeaponMechanicTester;
	}

	public void Update(float timestep)
	{
		_timeStep = timestep;
	}

	private IEnumerator<float> FunctionGen(float startX, float period)
	{
		float x = startX;
		while (true)
		{
			// f(x) : sin(x^ (min(.5x % p, -.5x % p) / .5d) function that looks like   - _/ \/\/VWV\/V\W
			x += _timeStep;

			yield return UnityEngine.Random.Range(-3f, 1f);

			//yield return Mathf.Sin(Mathf.Pow(x, (Mathf.Min((.5f * x) % period, (-.5f * x) % period + period)) / (.5f * period)));
		}
	}

	private bool InputDown(string button)
	{
		if (_buttonStates.TryGetValue(button, out IEnumerator<float> func))
		{
			func.MoveNext();
			return func.Current > 0f;
		}
		else
		{
			_buttonStates.Add(button, FunctionGen(UnityEngine.Random.Range(0, _period * 0.5f), _period));
			return InputDown(button);
		}
	}

	internal void ProccesInputNode(InputNode input, string button)
	{
		bool inputDown = InputDown(button);
		if (input.Held)
		{
			if (inputDown)
			{
				_playerWeaponMechanicTester.SetNodeActivity(null, input.InputHeld, true);
			}
			if (!inputDown)
			{
				_playerWeaponMechanicTester.SetNodeActivity(null, input.Input, true);
			}
		}
		else if (inputDown)
		{
			_playerWeaponMechanicTester.SetNodeActivity(null, input.Input, true);
		}
	}
}
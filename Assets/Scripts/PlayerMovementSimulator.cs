using UnityEngine;

internal class PlayerMovementSimulator : IPlayerMovement, IInputSim
{
	private PlayerWeaponMechanicTester _tester;
	private float _time;

	public PlayerMovementSimulator(PlayerWeaponMechanicTester tester)
	{
		_tester = tester;
	}

	public float SpeedMultiplier
	{
		get => 1f; set { }
	}

	public void Update(float timestep)
	{
		_time += timestep;
		_tester.MovedLastFrame = Mathf.Sin(_time) > 0f;
	}
}
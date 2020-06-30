internal class PlayerAtackInputSimulator : IPlayerAttackControl, IInputSim
{
	private float _animatingTimer = 0f;

	private float[] _animationTimes = new float[] { 0f, 1.04f, 0.53f, 0.71f,0F };
	private float[] _hitChance = new float[] { 0f, .9f, .9f, 0.7f, 0.7f };
	private Node _hitLastFrame = null;
	private PlayerWeaponMechanicTester _playerWeaponMechanicTester;

	public PlayerAtackInputSimulator(PlayerWeaponMechanicTester playerWeaponMechanicTester)
	{
		_playerWeaponMechanicTester = playerWeaponMechanicTester;
	}

	public bool ProccessAttackNode(float spd, float dmg, int type, Node node, int status)
	{
		if (_hitLastFrame != null)
		{
			_hitLastFrame = null;
			_playerWeaponMechanicTester.CollisionCallback(node);
		}

		if (_animatingTimer > 0f)
		{
			return false;
		}

		_animatingTimer = _animationTimes[type] / spd;
		//Debug.Log($"attack : type.{type} dmg.{dmg} ");
		MechanicBalancer.RegisterAttackObservation(node, dmg);

		if (_hitChance[type] > UnityEngine.Random.Range(0f, 1f))
		{
			_hitLastFrame = node;
		}

		return true;
	}

	public void Update(float timestep)
	{
		_animatingTimer -= timestep;
	}
}
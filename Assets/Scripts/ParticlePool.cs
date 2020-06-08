using UnityEngine;

[DefaultExecutionOrder(-9998)]
public class ParticlePool : MonoBehaviour
{
	private Animator[] _animators;
	private int _lastPlayedIndex;

	public void Spawn(Vector3 pos)
	{
		_animators[_lastPlayedIndex].transform.position = pos;
		_animators[_lastPlayedIndex].Play("Play");
		_lastPlayedIndex++;
		_lastPlayedIndex %= _animators.Length;
	}

	private void ParticlePool_OnRegisterHealth(HealthComponent healthComponent)
	{
		healthComponent.OnDie += () => Spawn(healthComponent.transform.position);
	}

	private void Start()
	{
		_animators = GetComponentsInChildren<Animator>();
		FindObjectOfType<GlobalHealthTracker>().OnRegisterHealth += ParticlePool_OnRegisterHealth;
	}
}

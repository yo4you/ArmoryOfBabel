using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class HurtSounds : MonoBehaviour
{
	[SerializeField]
	private ClipCollection DieClips;

	[SerializeField]
	private ClipCollection HitClips;

	private void Start()
	{
		GetComponent<HealthComponent>().OnHit += (f) =>
		{
			if (f != 0)
			{
				SoundManagerSingleton.Manager.PlayAudio(HitClips);
			}
		};

		GetComponent<HealthComponent>().OnDie += () => SoundManagerSingleton.Manager.PlayAudio(DieClips);
	}
}

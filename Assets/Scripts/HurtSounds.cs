using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class HurtSounds : MonoBehaviour
{
	[SerializeField]
	private ClipCollection DieClips = default;

	[SerializeField]
	private ClipCollection HitClips = default;

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
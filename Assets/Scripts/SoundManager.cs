using System;
using UnityEngine;

public static class SoundManagerSingleton
{
	private static SoundManager _manager = null;

	public static SoundManager Manager
	{
		get => _manager; set
		{
			if (_manager == null)
			{
				_manager = value;
			}
			else
			{
				throw new
					Exception("singleton already initialized");
			}
		}
	}
}

[DefaultExecutionOrder(-9999)]
public class SoundManager : MonoBehaviour
{
	private AudioSource[] _audioSources;
	private int _lastPlayedIndex;

	public void PlayAudio(AudioClip clip)
	{
		_lastPlayedIndex++;
		_lastPlayedIndex %= _audioSources.Length;
		_audioSources[_lastPlayedIndex].PlayOneShot(clip);
	}

	public void PlayAudio(ClipCollection clips)
	{
		if (clips.Clips.Count != 0)
		{
			PlayAudio(clips.Clips[UnityEngine.Random.Range(0, clips.Clips.Count)]);
		}
	}

	private void Awake()
	{
		SoundManagerSingleton.Manager = this;
	}

	private void Start()
	{
		_audioSources = GetComponentsInChildren<AudioSource>();
	}
}

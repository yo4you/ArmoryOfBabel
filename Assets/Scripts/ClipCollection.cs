using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// container for audioclips meant to be used with the audio manager
/// a random one of these clips will be played
/// </summary>
[Serializable]
public class ClipCollection
{
	[SerializeField]
	public List<AudioClip> Clips;
}
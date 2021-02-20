using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameJam
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent PlayerPrefab;
		public EntityComponent HelperPrefab;
		public ProjectileComponent DefaultProjectilePrefab;
		public List<Wave> Waves;

		[Header("Audio")]
		public AudioMixer AudioMixer;
		public AudioMixerGroup MusicAudioMixerGroup;
		public AudioMixerGroup SoundsAudioMixerGroup;
		public AudioMixerSnapshot DefaultAudioSnapshot;
		public AudioMixerSnapshot PauseAudioSnapshot;
		public AudioClip MainMusic;
		public AudioClip HelpReceivedMusic;
		public AudioClip MenuConfirmClip;
		[Range(0f, 1f)] public float MusicVolume = 1f;
		[Range(0f, 1f)] public float SoundVolume = 1f;
	}

	[Serializable]
	public class Wave
	{
		public List<Spawn> Spawns;
	}

	[Serializable]
	public class Spawn
	{
		public Vector2 Position;
		public EntityComponent EntityPrefab;
	}
}

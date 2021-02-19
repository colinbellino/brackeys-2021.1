using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameJam
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent LeaderPrefab;
		public ProjectileComponent DefaultProjectilePrefab;
		public List<Wave> Waves;

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
		[FormerlySerializedAs("Entity")] public EntityComponent EntityPrefab;
	}
}

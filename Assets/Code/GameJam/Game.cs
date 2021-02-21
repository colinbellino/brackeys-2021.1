using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
	public class Game
	{
		public GameConfig Config;
		public GameUI UI;
		public Camera Camera;
		public GameControls Controls;
		public GameState State;
		public ProjectileSpawner ProjectileSpawner;
		public AudioPlayer AudioPlayer;

		public static Vector3 PLAYER_SPAWN_POSITION = new Vector3(0f, -2f, 0f);
		public const float HELPERS_SPAWN_INTERVAL = 1f;
		public const float HELPERS_RADIUS = 1.5f;
		public const int HELPERS_MAX_COUNT = 5;

		public static Bounds Bounds = new Bounds(Vector3.zero, new Vector3(45f, 28f, 1f));
		public static Bounds MoveBounds = new Bounds(Vector3.zero, new Vector3(41f, 22f, 1f));
		public static readonly List<string> PlaceholderNames = new List<string>(new []{
			"Micah",
			"Vernon",
			"Rena",
			"Riku",
			"Andre",
			"Thea",
			"Mariel",
			"Jesse",
			"Marceline",
			"Gaius",
			"Alma",
			"Ursula",
			"Celeste",
			"Madeline",
			"Thea",
		});
	}
}

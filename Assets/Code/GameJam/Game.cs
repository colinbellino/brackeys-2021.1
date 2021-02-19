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

		public static Bounds Bounds = new Bounds(Vector3.zero, new Vector3(45f, 28f, 1f));
		public static Bounds MoveBounds = new Bounds(Vector3.zero, new Vector3(41f, 20f, 1f));
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

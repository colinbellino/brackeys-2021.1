using System.Collections.Generic;

namespace GameJam
{
	public class GameState
	{
		public EntityComponent Player;
		public readonly List<EntityComponent> Enemies = new List<EntityComponent>();
		public readonly List<ProjectileComponent> Projectiles = new List<ProjectileComponent>();
		public Queue<Wave> Waves;
		public bool HelpReceived;
		public List<string> HelpersName = new List<string>();
		public EntityComponent[] Helpers;
		public int DeathCounter;
	}
}

using System.Collections.Generic;

namespace GameJam
{
	public class GameState
	{
		public EntityComponent Leader;
		public readonly List<EntityComponent> Units = new List<EntityComponent>();
		public readonly List<ProjectileComponent> Projectiles = new List<ProjectileComponent>();
		public Queue<Wave> Waves;
		public List<string> Helpers = new List<string>();
		public int DeathCounter;
	}
}

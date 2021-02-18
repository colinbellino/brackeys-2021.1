using System.Collections.Generic;

namespace GameJam
{
	public class GameState
	{
		public List<EntityComponent> Units = new List<EntityComponent>();
		public EntityComponent Leader;
		public List<ProjectileComponent> Projectiles = new List<ProjectileComponent>();
	}
}

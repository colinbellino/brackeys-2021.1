using System.Collections.Generic;

namespace GameJam
{
	public class GameState
	{
		public List<EntityComponent> Units;
		public EntityComponent Leader;
		public List<ProjectileComponent> Projectiles;
		public Queue<Wave> Waves;
	}
}

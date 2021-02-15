using UnityEngine;

namespace GameJam
{
	[CreateAssetMenu(menuName = "Game Jam/Game Config")]
	public class GameConfig : ScriptableObject
	{
		public EntityComponent UnitPrefab;
		public EntityComponent ObstaclePrefab;
		public LayerMask SelectionMask;
	}
}

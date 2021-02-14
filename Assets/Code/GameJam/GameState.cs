using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameJam
{
	public class GameState
	{
		public List<Entity> Entities;
		public IEnumerable<Entity> Units => Entities.Where(entity => entity.Type == Entity.Types.Unit);
		public IEnumerable<Entity> Obstacles => Entities.Where(entity => entity.Type == Entity.Types.Obstacle);
		public List<Entity> SelectedUnits;

		public Vector3 SelectionStart;
		public Vector3 SelectionEnd;
		public bool IsSelectionInProgress;
	}
}

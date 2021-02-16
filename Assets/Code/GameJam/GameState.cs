using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameJam
{
	public class GameState
	{
		public List<Entity> Entities;
		public IEnumerable<Unit> Units => Entities.Where(entity => entity is Unit).Cast<Unit>();
		public IEnumerable<Obstacle> Obstacles => Entities.Where(entity => entity is Obstacle).Cast<Obstacle>();
		public List<Unit> SelectedUnits;

		public Vector3 SelectionStart;
		public Vector3 SelectionEnd;
		public bool IsSelectionInProgress;
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameJam
{
	public class GameState
	{
		public List<Unit> Units = new List<Unit>();
		public Unit Leader;
		public Queue<Unit> SelectedUnits;
		public List<Obstacle> Obstacles = new List<Obstacle>();

		public Vector3 SelectionStart;
		public Vector3 SelectionEnd;
		public bool IsSelectionInProgress;
	}
}

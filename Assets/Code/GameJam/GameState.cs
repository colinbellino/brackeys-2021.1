using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
	public class GameState
	{
		public List<Character> AllCharacters;
		public List<Character> SelectedCharacters;
		public bool SelectionInProgress;
		public Vector3 SelectionStart;
		public Vector3 SelectionEnd;
	}
}

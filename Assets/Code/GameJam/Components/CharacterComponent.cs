using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
	[SelectionBase]
	public class CharacterComponent : MonoBehaviour
	{
		[SerializeField] public Transform RootTransform;
		[SerializeField] public CharacterController CharacterController;
		[SerializeField] public GameObject Selection;
		[SerializeField] public Text DebugText;
	}

	public class Entity
	{
		public string Name;
		public Types Type;
		public CharacterComponent Component;

		// Units
		public float MoveSpeed = 5f;
		public Vector3 MoveDestination;
		public Entity ActionTarget;
		public States State;

		// Obstacles
		public int RequiredUnits;
		public float Duration;
		public float Progress;
		public Vector3 ObstacleDestination;

		public enum Types { None, Unit, Obstacle }
		public enum States { Idle, Moving, Acting, Inactive }

		public override string ToString() => Name;
	}
}

using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField] public Transform RootTransform;
		[SerializeField] public GameObject Selection;
		[SerializeField] public Text DebugText;
		[SerializeField] public AIPath AI;
		[SerializeField] public Rigidbody2D Rigidbody;
	}

	public class Entity
	{
		public static float MIN_MOVE_DISTANCE = 3f;

		public string Name;
		public Types Type;
		public EntityComponent Component;

		// Units
		public Vector3 MoveDestination;
		public Entity ActionTarget;
		public UnitStateMachine UnitStateMachine;

		// Obstacles
		public int RequiredUnits;
		public float Duration;
		public float Progress;
		public Vector3 ObstacleDestination;
		public ObstacleStateMachine ObstacleStateMachine;

		public enum Types { None, Unit, Obstacle }

		public override string ToString() => Name;
	}
}

using System.Collections.Generic;
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
		public EntityComponent Component;

		public override string ToString() => Name;
	}

	public class Obstacle : Entity
	{
		public int RequiredUnits;
		public float Duration;
		public float Progress;
		public Vector3 PushDestination;
		public ObstacleStateMachine StateMachine;
		public List<Entity> PushedBy = new List<Entity>();
	}

	public class Unit : Entity
	{
		public Vector3 MoveDestination;
		public Obstacle ActionTarget;
		public UnitStateMachine StateMachine;
	}
}

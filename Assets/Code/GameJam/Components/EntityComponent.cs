using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
	public enum Alliances { Foe, Ally }
	public enum AI { None, IdleShooter }

	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		public static float MIN_MOVE_DISTANCE = 3f;
		public static float MIN_FOLLOW_DISTANCE = 1f;

		[SerializeField] public GameObject Selection;
		[SerializeField] public Text DebugText;
		[SerializeField] public Rigidbody2D Rigidbody;
		[SerializeField] public ProjectileComponent ProjectilePrefab;
		[SerializeField] public float FireRate = 0.1f;
		[SerializeField] public float MoveSpeed = 10f;
		[SerializeField] public Alliances Alliance;
		[SerializeField] public AI Brain;
		[SerializeField] public Vector3 RotationPerTick;

		[HideInInspector] public int Health = 1	;
		[HideInInspector] public float CanFireTimestamp;
		[HideInInspector] public UnitStateMachine StateMachine;

		public override string ToString() => name;
	}

	// public class Entity
	// {
	// 	public static float MIN_MOVE_DISTANCE = 3f;
	// 	public static float MIN_FOLLOW_DISTANCE = 1f;
	//
	// 	public string Name;
	// 	public EntityComponent Component;
	// 	public float CanFireTimestamp;
	// 	public float MoveSpeed = 10f;
	//
	// 	public override string ToString() => Name;
	// }
	//
	// public class Obstacle : Entity
	// {
	// 	public int RequiredUnits;
	// 	public float Duration;
	// 	public float Progress;
	// 	public Vector3 PushDestination;
	// 	public ObstacleStateMachine StateMachine;
	// 	public List<Entity> PushedBy = new List<Entity>();
	// }
	//
	// public class Unit : Entity
	// {
	// 	public Vector3 MoveDestination;
	// 	public Obstacle ActionTarget;
	// 	public UnitStateMachine StateMachine;
	//
	// 	public float ThrowSpeed = 30f;
	// 	public Entity FollowTarget;
	// }
}

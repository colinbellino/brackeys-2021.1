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
		[SerializeField] public int StartingHealth = 3;
		[SerializeField] public Alliances Alliance;
		[SerializeField] public AI Brain;
		[SerializeField] public Vector3 RotationPerTick;

		[HideInInspector] public int Health = 1	;
		[HideInInspector] public float CanFireTimestamp;
		[HideInInspector] public UnitStateMachine StateMachine;

		public override string ToString() => name;
	}
}

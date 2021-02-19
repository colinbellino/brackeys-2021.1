using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
	public enum Alliances { Foe, Ally }
	public enum Brain { Player, IdleShooter, Roamer }

	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField] public float FireRate = 0.1f;
		[SerializeField] public float MoveSpeed = 10f;
		[SerializeField] public int StartingHealth = 3;
		[SerializeField] public Alliances Alliance;
		[SerializeField] public Brain Brain;
		[SerializeField] public Vector3 RotationPerTick;

		[HideInInspector] public int Health = 1	;
		[HideInInspector] public float CanFireTimestamp;
		[HideInInspector] public UnitStateMachine StateMachine;
		[HideInInspector] public Vector3 MoveDestination;
		[HideInInspector] public IEnumerable<ShooterComponent> Shooters;

		private void Awake()
		{
			Shooters = GetComponentsInChildren<ShooterComponent>();
		}
	}
}

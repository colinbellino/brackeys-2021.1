using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
	public enum Alliances { Foe, Ally }
	public enum Brain { Player, Helper, Shooter, Roamer }

	[SelectionBase]
	public class EntityComponent : MonoBehaviour
	{
		[SerializeField] public float FireRate = 0.1f;
		[SerializeField] public float MoveSpeed = 10f;
		[SerializeField] public float RotateSpeed = 5f;
		[SerializeField] public int StartingHealth = 3;
		[SerializeField] public Alliances Alliance;
		[SerializeField] public Brain Brain;
		[SerializeField] public Vector3 RotationPerTick;
		[SerializeField] public double InvulnerabilityDuration;
		[SerializeField] public SpriteRenderer[] Parts;
		[SerializeField] public AudioClip FireClip;
		[SerializeField] public AudioClip DamagedClip;
		[SerializeField] public AudioClip DestroyedClip;

		[HideInInspector] public int Health = 1;
		[HideInInspector] public float RotationOffset;
		[HideInInspector] public float CanFireTimestamp;
		[HideInInspector] public UnitStateMachine StateMachine;
		[HideInInspector] public Vector3 MoveDestination;
		[HideInInspector] public IEnumerable<ShooterComponent> Shooters;
		[HideInInspector] public int HelperIndex;
		[HideInInspector] public float HelperAngle;
		[HideInInspector] public double InvulnerabilityTimestamp;

		private void Awake()
		{
			Shooters = GetComponentsInChildren<ShooterComponent>();
		}
	}
}

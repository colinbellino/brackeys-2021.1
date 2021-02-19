using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GameJam
{
	public class ProjectileComponent : MonoBehaviour
	{
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public CircleCollider2D HitColliderRadius;

		[HideInInspector] public Alliances Alliance;
		[HideInInspector] public Projectile Data;

		public event Action Destroyed;

		private void Update()
		{
			transform.position = Vector3.Lerp(
				transform.position,
				transform.position + transform.up,
				Data.MoveSpeed * Time.deltaTime
			);

			var bounds = new Bounds(Vector3.zero, new Vector3(44f, 30f, 1f));
			if (bounds.Contains(transform.position) == false)
			{
				Destroyed?.Invoke();
			}
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			var entity = other.GetComponentInParent<EntityComponent>();
			if (entity != null && entity.Alliance != Alliance)
			{
				DestroyProjectile(this);
				HitEntity(entity, this);
				return;
			}

			var projectile = other.GetComponentInParent<ProjectileComponent>();
			if (projectile != null && projectile.Data.CanBeDestroyed && projectile.Alliance != Alliance)
			{
				DestroyProjectile(projectile);

				// var shouldDestroySelf = Random.Range(0, 100) > 75;
				if (Data.CanBeDestroyed)
				{
					DestroyProjectile(this);
				}
			}
		}

		public static void HitEntity(EntityComponent entity, ProjectileComponent projectile)
		{
			entity.Health -= 1;

			if (entity.Health <= 0)
			{
				entity.StateMachine.Fire(UnitStateMachine.Triggers.Destroyed);
			}
		}

		public static void DestroyProjectile(ProjectileComponent component)
		{
			component.Destroyed?.Invoke();
		}
	}
}

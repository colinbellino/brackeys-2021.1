using System;
using DG.Tweening;
using UnityEngine;

namespace GameJam
{
	public class ProjectileComponent : MonoBehaviour
	{
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public Light Light;
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

			if (Game.Bounds.Contains(transform.position) == false)
			{
				Destroyed?.Invoke();
			}
		}

		private void OnTriggerEnter2D(Collider2D collider)
		{
			var otherEntity = collider.GetComponentInParent<EntityComponent>();
			if (otherEntity != null && otherEntity.Alliance != Alliance)
			{
				HitEntity(otherEntity);

				HitProjectile(this);
				return;
			}

			var otherProjectile = collider.GetComponentInParent<ProjectileComponent>();
			if (otherProjectile != null && otherProjectile.Alliance != Alliance)
			{
				if (Data.CanDestroyOtherProjectiles && otherProjectile.Data.CanBeDestroyed)
				{
					HitProjectile(otherProjectile);
				}

				if (otherProjectile.Data.CanDestroyOtherProjectiles && Data.CanBeDestroyed)
				{
					HitProjectile(this);
				}
			}
		}

		public static void HitEntity(EntityComponent entity)
		{
			entity.Health -= 1;

			entity.Parts[0].transform.DOShakePosition(0.2f, 0.15f, 20, 90f, false, true);

			if (entity.Parts.Length > 0)
			{
				for (var partIndex = 0; partIndex < entity.Parts.Length; partIndex++)
				{
					var part = entity.Parts[partIndex];
					part.gameObject.SetActive(entity.Health > partIndex);
				}
			}

			if (entity.Health <= 0)
			{
				entity.StateMachine.Fire(UnitStateMachine.Triggers.Destroyed);
			}
		}

		public static void HitProjectile(ProjectileComponent component)
		{
			component.Destroyed?.Invoke();
		}
	}
}

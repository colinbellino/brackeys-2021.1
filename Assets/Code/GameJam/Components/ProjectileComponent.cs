using System;
using UnityEngine;
using static GameJam.Utils;

namespace GameJam
{
	public class ProjectileComponent : MonoBehaviour
	{
		[SerializeField] public SpriteRenderer SpriteRenderer;
		[SerializeField] public Light Light;
		[SerializeField] public CircleCollider2D HitColliderRadius;

		[HideInInspector] public Alliances Alliance;
		[HideInInspector] public Projectile Data;

		public Action Destroyed;

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
	}
}

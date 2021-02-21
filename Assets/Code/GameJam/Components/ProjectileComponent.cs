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

		private GameState _state;
		private AudioPlayer _audioPlayer;

		private void Awake()
		{
			var gameManager = FindObjectOfType<GameManager>();
			_state = gameManager.Game.State;
			_audioPlayer = gameManager.Game.AudioPlayer;
		}

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
			if (_state.Running == false)
			{
				return;
			}

			var otherEntity = collider.GetComponentInParent<EntityComponent>();
			if (otherEntity != null && otherEntity.Alliance != this.Alliance)
			{
				HitEntity(otherEntity, _audioPlayer);

				HitProjectile(this);
				return;
			}

			var otherProjectile = collider.GetComponentInParent<ProjectileComponent>();
			if (otherProjectile != null && otherProjectile.Alliance != this.Alliance)
			{
				if (this.Data.CanDestroyOtherProjectiles && otherProjectile.Data.CanBeDestroyed)
				{
					HitProjectile(otherProjectile);
				}

				if (otherProjectile.Data.CanDestroyOtherProjectiles && this.Data.CanBeDestroyed)
				{
					HitProjectile(this);
				}
			}
		}
	}
}

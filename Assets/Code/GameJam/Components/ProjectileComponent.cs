using UnityEngine;

namespace GameJam
{
	public class ProjectileComponent : MonoBehaviour
	{
		[SerializeField] public float MoveSpeed = 10f;
		[SerializeField] public bool CanBeDestroyed;

		[HideInInspector] public Alliances Alliance;

		private void Update()
		{
			transform.position = Vector3.Lerp(
				transform.position,
				transform.position + transform.up,
				MoveSpeed * Time.deltaTime
			);
		}

		private void OnTriggerEnter2D(Collider2D other)
		{
			var entity = other.GetComponentInParent<EntityComponent>();
			if (entity != null && entity.Alliance != Alliance)
			{
				Hit(entity, this);
				return;
			}

			var projectile = other.GetComponentInParent<ProjectileComponent>();
			if (projectile != null && projectile.CanBeDestroyed && projectile.Alliance != Alliance)
			{
				Destroy(projectile.gameObject);
			}
		}

		public static void Hit(EntityComponent entity, ProjectileComponent projectile)
		{
			entity.Health -= 1;

			if (entity.Health <= 0)
			{
				entity.StateMachine.Fire(UnitStateMachine.Triggers.Destroyed);
			}
		}
	}
}

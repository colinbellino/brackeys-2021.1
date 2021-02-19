using System;
using System.Collections.Generic;
using UnityEngine;
using static GameJam.Utils;

namespace GameJam
{
	public class ProjectileSpawner
	{
		private List<ProjectileComponent> _projectiles;

		public void CreatePool(int count, ProjectileComponent prefab)
		{
			_projectiles = new List<ProjectileComponent>();

			for (var i = 0; i < count; i++)
			{
				var projectile = GameObject.Instantiate(prefab);
				Disable(projectile);
				_projectiles.Add(projectile);
			}
		}

		public ProjectileComponent Spawn(EntityComponent entity, ShooterComponent shooter)
		{
			for (var i = 0; i < _projectiles.Count; i++)
			{
				if (_projectiles[i].gameObject.activeSelf)
				{
					continue;
				}

				Enable(_projectiles[i], entity, shooter);
				return _projectiles[i];
			}

			if (IsDevBuild())
			{
				throw new Exception("Projectile pool is full!");
			}
			Debug.LogError("Projectile pool is full!");
			return null;
		}

		public void Despawn(ProjectileComponent projectile)
		{
			Disable(projectile);
		}

		private static void Enable(ProjectileComponent projectile, EntityComponent entity, ShooterComponent shooter)
		{
			projectile.transform.position = shooter.transform.position;
			projectile.transform.rotation = shooter.transform.rotation;
			projectile.Alliance = entity.Alliance;
			projectile.Data = shooter.Projectile;
			projectile.SpriteRenderer.sprite = shooter.Projectile.Sprite;
			projectile.SpriteRenderer.color = shooter.Projectile.Color;
			projectile.HitColliderRadius.radius = shooter.Projectile.HitColliderRadius;
			projectile.gameObject.SetActive(true);
		}

		private static void Disable(ProjectileComponent projectile)
		{
			projectile.transform.position = Vector3.zero;
			projectile.gameObject.SetActive(false);
		}
	}
}

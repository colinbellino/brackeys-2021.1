﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static GameJam.Utils;
using Object = UnityEngine.Object;

namespace GameJam
{
	public class ProjectileSpawner
	{
		private List<ProjectileComponent> _projectiles;
		private GameObject _parent;

		public void CreatePool(int count, ProjectileComponent prefab)
		{
			_projectiles = new List<ProjectileComponent>();
			_parent = new GameObject("Projectiles");

			for (var i = 0; i < count; i++)
			{
				var projectile = GameObject.Instantiate(prefab, _parent.transform);
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
			projectile.transform.position = shooter.Origin.position;
			projectile.transform.rotation = shooter.transform.rotation;
			projectile.Alliance = entity.Alliance;
			projectile.Data = shooter.Projectile;
			projectile.SpriteRenderer.material = Object.Instantiate(shooter.Projectile.Material);
			projectile.SpriteRenderer.material.SetColor("_Color", shooter.Projectile.Color);
			projectile.SpriteRenderer.material.SetColor("_EmissionColor", shooter.Projectile.Color);
			projectile.SpriteRenderer.sprite = shooter.Projectile.Sprite;
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

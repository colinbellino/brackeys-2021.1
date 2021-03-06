﻿using UnityEngine;

namespace GameJam
{
	[CreateAssetMenu(menuName = "Game Jam/Projectile")]
	public class Projectile : ScriptableObject
	{
		public float MoveSpeed = 10f;
		public Sprite Sprite;
		public Material Material;
		[ColorUsage(true, true)] public Color Color = Color.white;
		public float HitColliderRadius = 0.3f;
		public bool CanBeDestroyed;
		public bool CanDestroyOtherProjectiles = true;
		public ParticleSystem HitParticle;
	}
}

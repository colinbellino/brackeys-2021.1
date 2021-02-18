using UnityEngine;

namespace GameJam
{
	public class ShooterComponent : MonoBehaviour
	{
		[SerializeField] public ProjectileComponent ProjectilePrefab;

		private void Awake()
		{
			var renderer = GetComponent<SpriteRenderer>();
			if (renderer != null)
			{
				renderer.enabled = false;
			}
		}
	}
}

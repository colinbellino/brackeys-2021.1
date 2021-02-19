using UnityEngine;

namespace GameJam
{
	public class ShooterComponent : MonoBehaviour
	{
		[SerializeField] public Projectile Projectile;

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

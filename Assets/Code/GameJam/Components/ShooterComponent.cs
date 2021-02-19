using UnityEngine;

namespace GameJam
{
	public class ShooterComponent : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _renderer;
		[SerializeField] public Transform Origin;

		[Header("Data")]
		[SerializeField] public Projectile Projectile;

		private void Awake()
		{
			if (_renderer != null)
			{
				_renderer.enabled = false;
			}
		}

		private void OnValidate()
		{
			_renderer.color = Projectile.Color;
			name = $"Shooter: {transform.rotation.eulerAngles.z}° - {Projectile.name}";
		}
	}
}

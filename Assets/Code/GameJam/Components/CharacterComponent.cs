using UnityEngine;

namespace GameJam
{
	[SelectionBase]
	public class CharacterComponent : MonoBehaviour
	{
		[SerializeField] public CharacterController CharacterController;
		[SerializeField] public Transform RootTransform;
		[SerializeField] public Transform HeadTransform;
		[SerializeField] public Transform GroundCheck;
		[SerializeField] public float GroundCheckRadius = 0.4f;
	}

	public class Character
	{
		public string Name { get; set; }
		public float RotationX { get; set; }
		public Vector3 Velocity { get; set; }
		public bool IsGrounded { get; set; }
		public bool IsBusy { get; set; }
		public bool WasPetted { get; set; }

		public CharacterComponent Component { get; set; }
	}
}

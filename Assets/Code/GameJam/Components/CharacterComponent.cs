using UnityEngine;

namespace GameJam
{
	[SelectionBase]
	public class CharacterComponent : MonoBehaviour
	{
		[SerializeField] public CharacterController CharacterController;
		[SerializeField] public Transform RootTransform;
		[SerializeField] public Transform GroundCheck;
		[SerializeField] public float GroundCheckRadius = 0.4f;
		[SerializeField] public GameObject Selection;
	}

	public class Character
	{
		public string Name;
		public float MoveSpeed = 10f;
		public Vector3 MoveDestination;
		public bool NeedsToMove;

		public CharacterComponent Component { get; set; }
	}
}

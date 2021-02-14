using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
	[SelectionBase]
	public class CharacterComponent : MonoBehaviour
	{
		[SerializeField] public Transform RootTransform;
		[SerializeField] public CharacterController CharacterController;
		[SerializeField] public GameObject Selection;
		[SerializeField] public Text DebugText;
	}

	public class Character
	{
		public string Name;
		public float MoveSpeed = 5f;
		public Vector3 MoveDestination;
		public Character ActionTarget;
		public bool NeedsToMove;
		public bool IsUnit;

		public CharacterComponent Component { get; set; }

		public override string ToString() => Name;
	}
}

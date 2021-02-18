using UnityEngine;

namespace GameJam
{
	public class UnitSpawner : MonoBehaviour
	{
		[SerializeField] public EntityComponent UnitPrefab;
		[SerializeField] public float Delay;
	}
}

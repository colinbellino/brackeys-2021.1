using UnityEngine;

namespace GameJam
{
	public class ObstacleSpawner : MonoBehaviour
	{
		public int RequiredUnits = 2;
		public float Duration = 2;
		public Vector3 PushDestination = new Vector3(2, 0, 0);
	}
}

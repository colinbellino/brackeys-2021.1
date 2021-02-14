using UnityEngine;
using static GameJam.Utils;

namespace GameJam
{
	public class LookAtCamera : MonoBehaviour
	{
		[SerializeField] private Transform _toRotate;

		private Game _game;
		private Quaternion _rotationOffset;

		private void Start()
		{
			_game = GetGameInstance();
			_rotationOffset = _toRotate.rotation;
		}

		private void Update()
		{
			var lookAtPosition = new Vector3(
				_game.Camera.transform.position.x,
				_toRotate.transform.position.y,
				_game.Camera.transform.position.z
			);

			_toRotate.LookAt(lookAtPosition);
			_toRotate.rotation *= _rotationOffset;
		}
	}
}

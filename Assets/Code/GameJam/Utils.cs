using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameJam
{
	public static class Utils
	{
		public static Game FindGameInstance()
		{
			var manager = GameObject.FindObjectOfType<GameManager>();
			if (manager == null)
			{
				throw new Exception("Couldn't find GameManager in scene.");
			}

			return manager.Game;
		}

        public static void Follow(Transform follower, Transform target)
        {
	        follower.position = target.position;
	        follower.rotation = target.rotation;
        }

        public static CharacterComponent GetCharacterPointedAt(Camera camera, float maxDistance, LayerMask interactionMask)
        {
	        var didHit = Physics.Raycast(
		        camera.transform.position, camera.transform.forward,
		        out var hit, maxDistance, interactionMask
	        );
	        if (didHit)
	        {
		        return hit.collider.GetComponentInParent<CharacterComponent>();
	        }

	        return null;
        }

        public static async UniTask AnimatePet(Transform transform)
        {
	        var originalRotation = transform.rotation;

			await DOTween.Sequence()
		        .Append(transform.DORotateQuaternion(originalRotation * Quaternion.Euler(new Vector3(-20f, 0f, 0f)), 0.2f))
		        .Append(transform.DORotateQuaternion(originalRotation, 0.1f))
		        .SetLoops(3)
	        ;
        }

        public static Character SpawnCharacter(CharacterComponent prefab, string name, Vector3 position, Quaternion rotation)
        {
	        var component = GameObject.Instantiate(prefab, position, rotation);
	        component.gameObject.name = name;
	        return new Character { Name = name, Component = component };
        }

        public static bool IsDevBuild()
        {
			#if UNITY_EDITOR
		        return true;
	        #endif

	        return false;
        }

        public static (Vector3, Vector3) GetSelectionBox(Vector3 start, Vector3 end)
        {
	        var size = new Vector3(end.x - start.x, end.y - start.y, 1f);
	        var origin = new Vector3(start.x + size.x / 2f, start.y + size.y / 2f, 0f);

	        if (start == end)
	        {
		        size.x = 0.1f;
		        size.y = 0.1f;
	        }

	        return (origin, size);
        }
	}
}

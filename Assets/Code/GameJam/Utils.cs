using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace GameJam
{
	public static class Utils
	{
		public static Game GetGameInstance()
		{
			var manager = GameObject.FindObjectOfType<GameManager>();
			if (manager == null)
			{
				throw new Exception("Couldn't find GameManager in scene.");
			}

			return manager.Game;
		}

		public static void Look(Character character, Camera camera, Vector2 lookInput, float sensitivity, float step)
		{
			var look = lookInput * (sensitivity * step);

			character.RotationX -= look.y;
			character.RotationX = Mathf.Clamp(character.RotationX, -90f, 90f);

			character.Component.HeadTransform.localRotation = Quaternion.Euler(character.RotationX, 0f, 0f);
			character.Component.RootTransform.Rotate(Vector3.up, look.x);
		}

		public static void Move(Character character, Vector2 moveInput, float speed, float gravityModifier, float step)
        {
            if (character.IsGrounded)
            {
                character.Velocity = Vector3.zero;
            }

            var move = (character.Component.RootTransform.forward * moveInput.y + character.Component.RootTransform.right * moveInput.x);
            character.Component.CharacterController.Move(move * (speed * step));

            character.Velocity += Physics.gravity * (gravityModifier * step * step);
            character.Component.CharacterController.Move(character.Velocity);
        }

        public static void UpdateIsGrounded(Character character, LayerMask groundCheckMask)
        {
            character.IsGrounded = Physics.CheckSphere(
                character.Component.GroundCheck.position,
                character.Component.GroundCheckRadius,
        		groundCheckMask
            );
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
        }
	}
}

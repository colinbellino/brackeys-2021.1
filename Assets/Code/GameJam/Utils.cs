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

        public static Entity SpawnUnit(CharacterComponent prefab, string name, Vector3 position)
        {
	        var component = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        component.gameObject.name = name;
	        return new Entity { Name = name, Component = component, Type = Entity.Types.Unit };
        }

        public static Entity SpawnObstacle(CharacterComponent prefab, string name, Vector3 position, int requiredUnits, int duration, Vector3 destination)
        {
	        var component = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        component.gameObject.name = name;
	        return new Entity { Name = name, Component = component, Type = Entity.Types.Obstacle, RequiredUnits = requiredUnits, Duration = duration, ObstacleDestination = destination};
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

        public static void SelectCharacter(CharacterComponent character, bool value)
        {
	        if (character.Selection != null)
	        {
		        character.Selection.SetActive(value);
	        }
        }

        public static void SetDebugText(CharacterComponent characterComponent, string value)
        {
	        if (characterComponent.DebugText != null)
	        {
		        characterComponent.DebugText.text = value;
	        }
        }

        public static void MoveTowards(Entity entity, Vector3 destination, float step)
        {
	        var motion = (destination - entity.Component.RootTransform.position).normalized;
	        entity.Component.CharacterController.Move(motion * step);
        }
	}
}

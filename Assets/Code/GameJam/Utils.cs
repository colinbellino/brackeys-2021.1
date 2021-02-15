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

        public static Entity SpawnUnit(EntityComponent prefab, string name, Vector3 position)
        {
	        var component = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        component.gameObject.name = name;
	        return new Entity { Name = name, Component = component, Type = Entity.Types.Unit };
        }

        public static Entity SpawnObstacle(EntityComponent prefab, string name, Vector3 position, int requiredUnits, int duration, Vector3 destination)
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

        public static void SelectCharacter(EntityComponent entity, bool value)
        {
	        if (entity.Selection != null)
	        {
		        entity.Selection.SetActive(value);
	        }
        }

        public static void SetDebugText(EntityComponent entityComponent, string value)
        {
	        if (entityComponent.DebugText != null)
	        {
		        entityComponent.DebugText.text = value;
	        }
        }
	}
}

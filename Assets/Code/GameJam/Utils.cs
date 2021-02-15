using System;
using System.Collections.Generic;
using Stateless;
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

        public static Entity SpawnUnit(EntityComponent prefab, Game game, string name, Vector3 position)
        {
	        var component = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        component.gameObject.name = name;
	        var entity = new Entity { Name = name, Component = component, Type = Entity.Types.Unit };
	        entity.UnitStateMachine = new UnitStateMachine(false, game, entity);
	        entity.UnitStateMachine.Start();
	        return entity;
        }

        public static Entity SpawnObstacle(EntityComponent prefab, Game game, string name, Vector3 position, int requiredUnits, int duration, Vector3 destination)
        {
	        var component = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        component.gameObject.name = name;
	        var entity = new Entity { Name = name, Component = component, Type = Entity.Types.Obstacle, RequiredUnits = requiredUnits, Duration = duration, ObstacleDestination = destination};
	        entity.ObstacleStateMachine = new ObstacleStateMachine(false, game, entity);
	        entity.ObstacleStateMachine.Start();
	        return entity;
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

        public static void OrderToMove(Entity entity, Vector3 destination, List<Entity> entities)
        {
	        if (Vector3.Distance(entity.Component.RootTransform.position, destination) > Entity.MIN_MOVE_DISTANCE)
	        {
		        entity.MoveDestination = destination;
	        }

	        var hit = Physics2D.CircleCast(destination, 0.5f, Vector2.zero);
	        if (hit.collider)
	        {
		        var targetComponent = hit.transform.GetComponentInParent<EntityComponent>();
		        var targetCharacter = entities.Find(entity => entity.Component == targetComponent);
		        if (targetCharacter?.Type == Entity.Types.Obstacle)
		        {
			        entity.ActionTarget = targetCharacter;
		        }
	        }
	        else
	        {
		        entity.ActionTarget = null;
	        }

	        if (entity.UnitStateMachine.CanFire(UnitStateMachine.Triggers.StartMoving))
	        {
		        entity.UnitStateMachine.Fire(UnitStateMachine.Triggers.StartMoving);
	        }
        }

        public static Entity GetEntity(List<Entity> entities, EntityComponent component)
        {
	        return entities.Find(character => character.Component == component);
        }
	}
}

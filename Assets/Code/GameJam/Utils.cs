using System;
using System.Collections.Generic;
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

        public static Unit SpawnUnit(EntityComponent prefab, Game game, UnitSpawner spawner)
        {
	        var component = GameObject.Instantiate(prefab, spawner.transform.position, Quaternion.identity);
	        var entity = new Unit { Name = component.name, Component = component };
	        entity.StateMachine = new UnitStateMachine(true, game, entity);
	        entity.StateMachine.Start();
	        SelectCharacter(component, false);
	        SetDebugText(component, "");
	        GameObject.Destroy(spawner.gameObject);
	        return entity;
        }

        public static Unit SpawnLeader(EntityComponent prefab, Game game, LeaderSpawner spawner)
        {
	        var component = GameObject.Instantiate(prefab, spawner.transform.position, Quaternion.identity);
	        component.transform.name = "Leader";
	        var entity = new Unit { Name = component.transform.name, Component = component };
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
	        entity.StateMachine.Start();
	        SelectCharacter(component, false);
	        SetDebugText(component, "");
	        GameObject.Destroy(spawner.gameObject);
	        return entity;
        }

        public static Obstacle SpawnObstacle(EntityComponent prefab, Game game, ObstacleSpawner spawner)
        {
	        var component = GameObject.Instantiate(prefab, spawner.transform.position, Quaternion.identity);
	        var entity = new Obstacle
	        {
		        Name = component.transform.name, Component = component,
		        RequiredUnits = spawner.RequiredUnits, Duration = spawner.Duration,
		        PushDestination = spawner.PushDestination,
	        };
	        entity.StateMachine = new ObstacleStateMachine(false, game, entity);
	        entity.StateMachine.Start();
	        SelectCharacter(component, false);
	        SetDebugText(component, "");
	        GameObject.Destroy(spawner.gameObject);
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

        public static void OrderToMove(Unit entity, Vector3 destination, List<Entity> entities)
        {
			var hit = Physics2D.CircleCast(destination, 0.5f, Vector2.zero);
	        if (hit.collider)
	        {
		        var targetComponent = hit.transform.GetComponentInParent<EntityComponent>();
		        var targetEntity = entities.Find(entity => entity.Component == targetComponent);
		        if (targetEntity is Obstacle obstacle)
		        {
			        entity.ActionTarget = obstacle;
		        }
	        }

	        if (Vector3.Distance(entity.Component.RootTransform.position, destination) > Entity.MIN_MOVE_DISTANCE)
	        {
		        entity.MoveDestination = destination;
		        entity.StateMachine.Fire(UnitStateMachine.Triggers.Thrown);
	        }
        }

        public static Entity GetEntity(List<Entity> entities, EntityComponent component)
        {
	        return entities.Find(character => character.Component == component);
        }

        public static Unit GetEntity(List<Unit> entities, EntityComponent component)
        {
	        return entities.Find(character => character.Component == component);
        }

        public static Obstacle GetEntity(List<Obstacle> entities, EntityComponent component)
        {
	        return entities.Find(character => character.Component == component);
        }

        public static Vector3 GetMouseWorldPosition(GameControls controls, Camera camera)
        {
	        var mousePosition = controls.Gameplay.MousePosition.ReadValue<Vector2>();
	        var mouseWorldPosition = camera.ScreenToWorldPoint(mousePosition);
	        mouseWorldPosition.z = 0f;
	        return mouseWorldPosition;
        }

        public static void TryPushObstacles(Unit actor, Game game)
        {
	        var hits = Physics2D.CircleCastAll(actor.Component.RootTransform.position, radius: 4f, Vector2.zero, 0f);
	        foreach (var hit in hits)
	        {
		        var entityComponent = hit.transform.GetComponentInParent<EntityComponent>();
		        var obstacle = GetEntity(game.State.Obstacles, entityComponent);
		        if (obstacle != null)
		        {
			        if (obstacle.StateMachine.CanFire(ObstacleStateMachine.Triggers.StartMoving))
			        {
				        actor.ActionTarget = obstacle;
				        obstacle.StateMachine.Fire(ObstacleStateMachine.Triggers.StartMoving);
				        actor.StateMachine.Fire(UnitStateMachine.Triggers.StartPushing);
			        }
		        }
	        }
        }

        public static void TryFollowLeader(Unit actor, Game game)
        {
	        var hits = Physics2D.CircleCastAll(actor.Component.RootTransform.position, radius: 3f, Vector2.zero, 0f);
	        foreach (var hit in hits)
	        {
		        var entityComponent = hit.transform.GetComponentInParent<EntityComponent>();
		        if (game.State.Leader.Component == entityComponent)
		        {
			        if (actor.StateMachine.CanFire(UnitStateMachine.Triggers.StartFollowing))
			        {
				        actor.FollowTarget = game.State.Leader;
				        actor.StateMachine.Fire(UnitStateMachine.Triggers.StartFollowing);
				        game.State.SelectedUnits.Enqueue(actor);
			        }
		        }
	        }
        }
	}
}

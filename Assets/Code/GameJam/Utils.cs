using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public static async UniTask<EntityComponent> SpawnEnemy(EntityComponent prefab, Game game, Vector3 position)
        {
	        var origin = position;
	        origin.y = Game.Bounds.max.y;
	        var entity = GameObject.Instantiate(prefab, origin, Quaternion.identity);
	        entity.StateMachine = new UnitStateMachine(true, game, entity);
	        entity.MoveDestination = position;
	        entity.Health = entity.StartingHealth;
	        await entity.StateMachine.Start();
	        return entity;
        }

        public static async UniTask<EntityComponent> SpawnPlayer(EntityComponent prefab, Game game, Vector3 position)
        {
	        var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        entity.transform.name = "Leader";
	        entity.Health = entity.StartingHealth;
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
	        await entity.StateMachine.Start();
	        return entity;
        }

        public static async UniTask<EntityComponent> SpawnHelper(EntityComponent prefab, float offset, string name, Game game, Vector3 position)
        {
	        var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        entity.transform.name = $"Helper: {name}";
	        entity.Health = 1;
	        entity.MoveDestination = game.State.Player.transform.position;
	        entity.RotationOffset = offset;
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
	        await entity.StateMachine.Start();
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

        public static Vector3 GetMouseWorldPosition(GameControls controls, Camera camera)
        {
	        var mousePosition = controls.Gameplay.MousePosition.ReadValue<Vector2>();
	        var mouseWorldPosition = camera.ScreenToWorldPoint(mousePosition);
	        mouseWorldPosition.z = 0f;
	        return mouseWorldPosition;
        }

        public static void FireProjectile(EntityComponent entity, GameState state, ProjectileSpawner projectileSpawner)
        {
	        if (Time.time < entity.CanFireTimestamp)
	        {
		        return;
	        }


	        foreach (var shooter in entity.Shooters)
	        {
		        var projectile = projectileSpawner.Spawn(entity, shooter);
		        projectile.Destroyed += () => projectileSpawner.Despawn(projectile);
		        state.Projectiles.Add(projectile);
	        }

	        entity.CanFireTimestamp = Time.time + entity.FireRate;
        }

        public static async UniTask<List<string>> LoadHelpers()
        {
	        await UniTask.NextFrame();
	        return Game.PlaceholderNames;
        }
	}
}

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

        public static async UniTask<EntityComponent> SpawnUnit(EntityComponent prefab, Game game, UnitSpawner spawner)
        {
	        var origin = spawner.transform.position;
	        origin.y = 23;
	        var entity = GameObject.Instantiate(prefab, origin, Quaternion.identity);
	        entity.StateMachine = new UnitStateMachine(true, game, entity);
	        entity.MoveDestination = spawner.transform.position;
	        entity.Health = entity.StartingHealth;
	        await entity.StateMachine.Start();
	        SelectCharacter(entity, false);
	        SetDebugText(entity, "");
	        return entity;
        }

        public static async UniTask<EntityComponent> SpawnLeader(EntityComponent prefab, Game game, LeaderSpawner spawner)
        {
	        var entity = GameObject.Instantiate(prefab, spawner.transform.position, Quaternion.identity);
	        entity.transform.name = "Leader";
	        entity.Health = entity.StartingHealth;
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
	        await entity.StateMachine.Start();
	        SelectCharacter(entity, false);
	        SetDebugText(entity, "");
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

        public static Vector3 GetMouseWorldPosition(GameControls controls, Camera camera)
        {
	        var mousePosition = controls.Gameplay.MousePosition.ReadValue<Vector2>();
	        var mouseWorldPosition = camera.ScreenToWorldPoint(mousePosition);
	        mouseWorldPosition.z = 0f;
	        return mouseWorldPosition;
        }

        public static void FireProjectile(EntityComponent entity, GameState state)
        {
	        if (Time.time < entity.CanFireTimestamp)
	        {
		        return;
	        }

	        var projectile = GameObject.Instantiate(entity.ProjectilePrefab, entity.transform.position, entity.transform.rotation);
	        projectile.Alliance = entity.Alliance;

	        state.Projectiles.Add(projectile);

	        entity.CanFireTimestamp = Time.time + entity.FireRate;
        }

        public static async UniTask MoveInPosition(EntityComponent entity)
        {
	        await entity.transform.DOMove(entity.MoveDestination, 2f).SetEase(Ease.InSine);
        }
	}
}

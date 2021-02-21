using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;

namespace GameJam
{
	public static class Utils
	{
        public static async UniTask<EntityComponent> SpawnEnemy(EntityComponent prefab, Game game, Vector3 position)
        {
	        var origin = position;
	        origin.y = Game.Bounds.max.y;
	        var entity = GameObject.Instantiate(prefab, origin, Quaternion.identity);
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
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
	        entity.MoveDestination = Game.PLAYER_SPAWN_POSITION;
	        entity.StateMachine = new UnitStateMachine(false, game, entity);
	        await entity.StateMachine.Start();
	        return entity;
        }

        public static async UniTask<EntityComponent> SpawnHelper(EntityComponent prefab, int index, string name, Game game, Vector3 position)
        {
	        var entity = GameObject.Instantiate(prefab, position, Quaternion.identity);
	        entity.transform.name = $"Helper: {name}";
	        entity.Health = 1;
	        entity.MoveDestination = game.State.Player.transform.position;
	        entity.HelperIndex = index;
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

        public static Vector3 GetMouseWorldPosition(GameControls controls, Camera camera)
        {
	        var mousePosition = controls.Gameplay.MousePosition.ReadValue<Vector2>();
	        var mouseWorldPosition = camera.ScreenToWorldPoint(mousePosition);
	        mouseWorldPosition.z = 0f;
	        return mouseWorldPosition;
        }

        public static void FireProjectile(EntityComponent entity, GameState state, AudioPlayer audioPlayer, ProjectileSpawner projectileSpawner)
        {
	        if (entity == null || Time.time < entity.CanFireTimestamp)
	        {
		        return;
	        }

	        if (entity.FireClip)
	        {
		        _ = audioPlayer.PlaySoundEffect(entity.FireClip);
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
	        try
	        {
		        var apiUrl = IsDevBuild()
			        ? "http://localhost:8888/.netlify/functions/helpers"
			        : "https://brackeys-2021-1.netlify.app/.netlify/functions/helpers";
		        var request = await UnityWebRequest.Get(apiUrl).SendWebRequest();
		        var text = request.downloadHandler.text;
		        var response = JsonUtility.FromJson<APIHelpersResponse>(text);
		        return new List<string>(response.authors);
	        }
	        catch (Exception)
	        {
		        Debug.Log("Couldn't load data from server, falling back to placeholders.");
		        return Game.PlaceholderNames;
	        }
        }

        public static void HitEntity(EntityComponent entity, AudioPlayer audioPlayer)
        {
	        if (Time.time < entity.InvulnerabilityTimestamp)
	        {
		        return;
	        }

	        entity.Health -= 1;
	        entity.InvulnerabilityTimestamp = Time.time + entity.InvulnerabilityDuration;

	        entity.Parts[0].transform.DOShakePosition(0.2f, 0.15f, 20, 90f, false, true);

	        if (entity.Parts.Length > 0)
	        {
		        for (var partIndex = 0; partIndex < entity.Parts.Length; partIndex++)
		        {
			        var part = entity.Parts[partIndex];
			        part.gameObject.SetActive(entity.Health > partIndex);
		        }
	        }

	        if (entity.Health <= 0)
	        {
		        if (entity.DestroyedClip)
		        {
			        _ = audioPlayer.PlaySoundEffect(entity.DestroyedClip);
		        }

		        entity.StateMachine.Fire(UnitStateMachine.Triggers.Destroyed);
	        }
	        else
	        {
		        if (entity.DamagedClip)
		        {
			        _ = audioPlayer.PlaySoundEffect(entity.DamagedClip);
		        }
	        }
        }

        public static void HitProjectile(ProjectileComponent component)
        {
	        component.Destroyed?.Invoke();
        }
	}
}

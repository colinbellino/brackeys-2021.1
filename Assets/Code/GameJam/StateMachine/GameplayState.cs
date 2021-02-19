using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static GameJam.Utils;

namespace GameJam
{
	public class GameplayState : BaseGameState
	{
		private double _startedTimestamp;

		public GameplayState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Units = new List<EntityComponent>();
			_state.Projectiles = new List<ProjectileComponent>();
			_state.Waves = new Queue<Wave>(_config.Waves);

			if (IsDevBuild())
			{
				if (SceneManager.sceneCount > 1)
				{
					await SceneManager.UnloadSceneAsync("Level");
				}
			}
			await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);

			{
				var spawner = GameObject.FindObjectOfType<LeaderSpawner>();
				_state.Leader = await SpawnLeader(_config.LeaderPrefab, _game, spawner);
				GameObject.Destroy(spawner.gameObject);
			}

			_projectileSpawner.CreatePool(200, _config.DefaultProjectilePrefab);

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.ConfirmPress.performed += OnConfirmPressed;
			_controls.Gameplay.Confirm.performed += OnConfirmReleased;
			_controls.Gameplay.Cancel.performed += OnCancelReleased;

			_startedTimestamp = Time.time;
		}

		public override async void Tick()
		{
			base.Tick();

			if (_startedTimestamp == 0f)
			{
				return;
			}

			for (var entityIndex = _state.Units.Count - 1; entityIndex >= 0; entityIndex--)
			{
				var entity = _state.Units[entityIndex];
				if (entity == null)
				{
					_state.Units.RemoveAt(entityIndex);
					continue;
				}

				entity.StateMachine.Tick();
			}

			if (_state.Units.Count == 0)
			{
				if (_state.Waves.Count == 0)
				{
					_machine.Fire(GameStateMachine.Triggers.Victory);
					return;
				}

				var wave = _state.Waves.Dequeue();

				foreach (var spawn in wave.Spawns)
				{
					var unit = await SpawnUnit(spawn.EntityPrefab, _game, spawn.Position);
					_state.Units.Add(unit);
				}
			}

			if (_state.Leader == null)
			{
				_machine.Fire(GameStateMachine.Triggers.Defeat);
			}
			else
			{
				_ui.SetDebugText($"Player health: {_state.Leader.Health}");
				_state.Leader.StateMachine.Tick();
			}

			if (IsDevBuild())
			{
				if (Keyboard.current.f1Key.wasPressedThisFrame)
				{
					_machine.Fire(GameStateMachine.Triggers.Victory);
				}
				if (Keyboard.current.f2Key.wasPressedThisFrame)
				{
					_machine.Fire(GameStateMachine.Triggers.Defeat);
				}
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			await SceneManager.UnloadSceneAsync("Level");

			if (_state.Leader != null)
			{
				GameObject.Destroy(_state.Leader.gameObject);
			}

			foreach (var unit in _state.Units)
			{
				GameObject.Destroy(unit.gameObject);
			}
			foreach (var projectile in _state.Projectiles)
			{
				_projectileSpawner.Despawn(projectile);
			}

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.ConfirmPress.performed -= OnConfirmPressed;
			_controls.Gameplay.Confirm.performed -= OnConfirmReleased;
			_controls.Gameplay.Cancel.performed -= OnCancelReleased;

			_startedTimestamp = 0f;
		}

		private void OnConfirmPressed(InputAction.CallbackContext obj) { }

		private void OnConfirmReleased(InputAction.CallbackContext obj) { }

		private void OnCancelReleased(InputAction.CallbackContext obj) { }
	}
}


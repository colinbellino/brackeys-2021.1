using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameJam.Utils;

namespace GameJam
{
	public class GameplayState : BaseGameState
	{
		private double _startedTimestamp;
		private double _spawnHelperTimestamp;

		public GameplayState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Enemies.Clear();
			_state.Projectiles.Clear();
			_state.Waves = new Queue<Wave>(_config.Waves);
			_state.Helpers = new EntityComponent[5];
			_state.Player = await SpawnPlayer(_config.PlayerPrefab, _game, Vector3.zero);

			_ui.ShowGameplay();
			_ui.SetDebugText("");

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.performed += OnConfirmPerformed;
			_controls.Gameplay.Move.performed += OnConfirmPerformed;
		}

		public override async void Tick()
		{
			base.Tick();

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
				if (Keyboard.current.f3Key.wasPressedThisFrame)
				{
					_state.DeathCounter = 99;
					_machine.Fire(GameStateMachine.Triggers.Defeat);
				}
			}

			if (_startedTimestamp == 0f)
			{
				return;
			}

			for (var entityIndex = _state.Enemies.Count - 1; entityIndex >= 0; entityIndex--)
			{
				var entity = _state.Enemies[entityIndex];
				if (entity == null)
				{
					_state.Enemies.RemoveAt(entityIndex);
					continue;
				}

				entity.StateMachine.Tick();
			}

			for (var entityIndex = _state.Helpers.Length - 1; entityIndex >= 0; entityIndex--)
			{
				var entity = _state.Helpers[entityIndex];
				if (entity != null)
				{
					entity.StateMachine.Tick();
				}
			}

			if (_state.Enemies.Count == 0)
			{
				if (_state.Waves.Count == 0)
				{
					_machine.Fire(GameStateMachine.Triggers.Victory);
					return;
				}

				var wave = _state.Waves.Dequeue();

				foreach (var spawn in wave.Spawns)
				{
					var enemy = await SpawnEnemy(spawn.EntityPrefab, _game, spawn.Position);
					_state.Enemies.Add(enemy);
				}
			}

			if (_state.HelpReceived && Time.time > _spawnHelperTimestamp)
			{
				for (var helperIndex = 0; helperIndex < _state.Helpers.Length; helperIndex++)
				{
					if (_state.Helpers[helperIndex] != null)
					{
						continue;
					}

					var helperName = _state.HelpersName[Random.Range(0, _state.HelpersName.Count)];
					var position = new Vector3(0f, Game.Bounds.min.y, 0f);
					var entity = await SpawnHelper(_config.HelperPrefab, helperIndex, helperName, _game, position);
					_state.Helpers[helperIndex] = entity;

					if (helperIndex + 1 < Game.HELPERS_MAX_COUNT)
					{
						_spawnHelperTimestamp = Time.time + Game.HELPERS_SPAWN_INTERVAL;
					}

					break;
				}
			}

			if (_state.Player == null)
			{
				_machine.Fire(GameStateMachine.Triggers.Defeat);
			}
			else
			{
				_ui.SetDebugText($"Player health: {_state.Player.Health}");
				_state.Player.StateMachine.Tick();
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			await _ui.StartFadeToBlack();

			if (_state.Player != null)
			{
				GameObject.Destroy(_state.Player.gameObject);
			}

			foreach (var helper in _state.Helpers)
			{
				if (helper != null)
				{
					GameObject.Destroy(helper.gameObject);
				}
			}

			foreach (var enemy in _state.Enemies)
			{
				GameObject.Destroy(enemy.gameObject);
			}
			_state.Enemies.Clear();

			foreach (var projectile in _state.Projectiles)
			{
				_projectileSpawner.Despawn(projectile);
			}
			_state.Projectiles.Clear();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.performed -= OnConfirmPerformed;
			_controls.Gameplay.Move.performed -= OnConfirmPerformed;

			_startedTimestamp = 0f;
		}

		private void OnConfirmPerformed(InputAction.CallbackContext obj)
		{
			_startedTimestamp = Time.time;

			_controls.Gameplay.Confirm.performed -= OnConfirmPerformed;
			_controls.Gameplay.Move.performed -= OnConfirmPerformed;
		}
	}
}


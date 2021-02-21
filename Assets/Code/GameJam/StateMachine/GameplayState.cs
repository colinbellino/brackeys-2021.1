using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameJam.Utils;

namespace GameJam
{
	public class GameplayState : BaseGameState
	{
		private double _spawnHelperTimestamp;
		private bool _transitionDone;

		public GameplayState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Enemies.Clear();
			_state.Projectiles.Clear();
			_state.Waves = new Queue<Wave>(_config.Waves);
			_state.Helpers = new EntityComponent[5];

			if (IsDevBuild())
			{
				_ui.ShowGameplay();
				_ui.SetDebugText("DEBUG MENU \n- F1: Trigger win \n- F2: Trigger loss \n- F3: Trigger loss (with helpers)");
			}

			if (_state.DeathCounter > 0)
			{
				_state.Player = await SpawnPlayer(_config.PlayerPrefab, _game, Game.PLAYER_SPAWN_POSITION);
			}

			if (_audioPlayer.IsMusicPlaying() == false)
			{
				var music = _state.HelpReceived ? _config.HelpReceivedMusic : _config.MainMusic;
				_ = _audioPlayer.PlayMusic(music, false, 0.5f);
			}

			await _ui.EndFadeToBlack();
			if (_state.DeathCounter == 0)
			{
				_state.Player = await SpawnPlayer(_config.PlayerPrefab, _game, new Vector3(0f, Game.Bounds.min.y, 0f));
				await UniTask.Delay(3000); // Wait for player to move in position
			}

			_spawnHelperTimestamp = Time.time + Game.HELPERS_SPAWN_INTERVAL;
			_transitionDone = true;

			_controls.Gameplay.Enable();
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

			if (_transitionDone == false)
			{
				return;
			}

			for (var entityIndex = _state.Helpers.Length - 1; entityIndex >= 0; entityIndex--)
			{
				var entity = _state.Helpers[entityIndex];
				if (entity != null)
				{
					entity.StateMachine.Tick();
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

			if (_state.Player == null)
			{
				_machine.Fire(GameStateMachine.Triggers.Defeat);
			}
			else
			{
				_state.Player.StateMachine.Tick();
			}
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_transitionDone = false;

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
				if (enemy != null)
				{
					GameObject.Destroy(enemy.gameObject);
				}
			}
			_state.Enemies.Clear();

			foreach (var projectile in _state.Projectiles)
			{
				_projectileSpawner.Despawn(projectile);
			}
			_state.Projectiles.Clear();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
		}
	}
}


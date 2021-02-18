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
		private bool _loaded;
		public GameplayState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Units = new List<EntityComponent>();
			_state.Projectiles = new List<ProjectileComponent>();

			await SceneManager.LoadSceneAsync("Level", LoadSceneMode.Additive);

			{
				var spawner = GameObject.FindObjectOfType<LeaderSpawner>();
				_state.Leader = await SpawnLeader(_config.LeaderPrefab, _game, spawner);
			}

			foreach (var spawner in GameObject.FindObjectsOfType<UnitSpawner>())
			{
				_state.Units.Add(await SpawnUnit(_config.UnitPrefab, _game, spawner));
			}

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.ConfirmPress.performed += OnConfirmPressed;
			_controls.Gameplay.Confirm.performed += OnConfirmReleased;
			_controls.Gameplay.Cancel.performed += OnCancelReleased;

			_loaded = true;
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
				if (unit != null)
				{
					GameObject.Destroy(unit.gameObject);
				}
			}
			foreach (var projectile in _state.Projectiles)
			{
				if (projectile != null)
				{
					GameObject.Destroy(projectile.gameObject);
				}
			}

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.ConfirmPress.performed -= OnConfirmPressed;
			_controls.Gameplay.Confirm.performed -= OnConfirmReleased;
			_controls.Gameplay.Cancel.performed -= OnCancelReleased;

			_loaded = false;
		}

		public override void Tick()
		{
			base.Tick();

			if (_loaded == false)
			{
				return;
			}

			if (_state.Leader == null)
			{
				_machine.Fire(GameStateMachine.Triggers.Defeat);
			}
			else
			{
				_state.Leader.StateMachine.Tick();
			}

			foreach (var entity in _state.Units)
			{
				entity.StateMachine?.Tick();
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

		private void OnConfirmPressed(InputAction.CallbackContext obj) { }

		private void OnConfirmReleased(InputAction.CallbackContext obj) { }

		private void OnCancelReleased(InputAction.CallbackContext obj) { }
	}
}


using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameJam.Utils;

namespace GameJam
{
	public class GameplayState : BaseGameState
	{
		public GameplayState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Units = new List<Unit>();
			_state.Obstacles = new List<Obstacle>();
			_state.SelectedUnits = new Queue<Unit>();

			{
				var spawner = GameObject.FindObjectOfType<LeaderSpawner>();
				_state.Leader = SpawnLeader(_config.LeaderPrefab, _game, spawner);
			}

			foreach (var spawner in GameObject.FindObjectsOfType<UnitSpawner>())
			{
				_state.Units.Add(SpawnUnit(_config.UnitPrefab, _game, spawner));
			}

			foreach (var spawner in GameObject.FindObjectsOfType<ObstacleSpawner>())
			{
				_state.Obstacles.Add(SpawnObstacle(_config.ObstaclePrefab, _game, spawner));
			}

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.ConfirmPress.performed += OnConfirmPressed;
			_controls.Gameplay.Confirm.performed += OnConfirmReleased;
			_controls.Gameplay.Cancel.performed += OnCancelReleased;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.ConfirmPress.performed -= OnConfirmPressed;
			_controls.Gameplay.Confirm.performed -= OnConfirmReleased;
			_controls.Gameplay.Cancel.performed -= OnCancelReleased;
		}

		public override void Tick()
		{
			base.Tick();

			var mouseWorldPosition = GetMouseWorldPosition(_controls, _camera);
			_ui.MoveCursor(mouseWorldPosition);

			_camera.transform.position = new Vector3(
				_state.Leader.Component.RootTransform.position.x,
				_state.Leader.Component.RootTransform.position.y,
				_camera.transform.position.z
			);

			var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
			if (moveInput.magnitude > 0f)
			{
				_state.Leader.Component.Rigidbody.velocity = Vector3.zero;
				_state.Leader.Component.RootTransform.position = Vector3.Lerp(
					_state.Leader.Component.RootTransform.position,
					_state.Leader.Component.RootTransform.position + new Vector3(moveInput.x, moveInput.y, 0f),
					_state.Leader.MoveSpeed * Time.deltaTime
				);
			}

			foreach (var entity in _state.Units)
			{
				entity.StateMachine?.Tick();
			}

			foreach (var entity in _state.Obstacles)
			{
				entity.StateMachine?.Tick();
			}

			_ui.SetSelectedUnits(_state.SelectedUnits.ToList());

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

		private void OnConfirmPressed(InputAction.CallbackContext obj)
		{

		}

		private void OnConfirmReleased(InputAction.CallbackContext obj)
		{
			if (_state.SelectedUnits.Count == 0)
			{
				return;
			}

			var mouseWorldPosition = GetMouseWorldPosition(_controls, _camera);
			var unit = _state.SelectedUnits.Dequeue();

			unit.MoveDestination = mouseWorldPosition;
			unit.StateMachine.Fire(UnitStateMachine.Triggers.Thrown);
		}

		private void OnCancelReleased(InputAction.CallbackContext obj)
		{

		}
	}
}

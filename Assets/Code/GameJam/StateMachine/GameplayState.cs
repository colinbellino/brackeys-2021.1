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

		private const float MIN_MOVE_DISTANCE = 1f;

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Entities = new List<Entity>();
			_state.SelectedUnits = new List<Entity>();

			var character1 = SpawnUnit(_config.UnitPrefab, "Ariette", new Vector3(0f, 0f, 0f));
			_state.Entities.Add(character1);
			var character2 = SpawnUnit(_config.UnitPrefab, "Joe", new Vector3(3f, 2f, 0f));
			_state.Entities.Add(character2);
			var character3 = SpawnUnit(_config.UnitPrefab, "Jessi", new Vector3(-5f, -2f, 0f));
			_state.Entities.Add(character3);

			var obstacle1 = SpawnObstacle(_config.ObstaclePrefab, "Obstacle1", new Vector3(9f, 7f, 0f), 2, 2, new Vector3(11f, 7f, 0f));
			_state.Entities.Add(obstacle1);
			var obstacle2 = SpawnObstacle(_config.ObstaclePrefab, "Obstacle2", new Vector3(5f, -3f, 0f), 1, 2, new Vector3(5f, -1f, 0f));
			_state.Entities.Add(obstacle2);

			foreach (var character in _state.Entities)
			{
				SelectCharacter(character.Component, false);
				SetDebugText(character.Component, "");
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

			foreach (var entity in _state.Units)
			{
				switch (entity.State)
				{
					case Entity.States.Moving:
					{
						MoveTowards(entity, entity.MoveDestination, entity.MoveSpeed * Time.deltaTime);

						if (Vector3.Distance(entity.Component.RootTransform.position, entity.MoveDestination) < MIN_MOVE_DISTANCE)
						{
							entity.State = Entity.States.Acting;
						}
					} break;

					case Entity.States.Acting:
					{
						if (entity.ActionTarget != null)
						{
							// Debug.Log(entity.Name + " interacting with " + entity.ActionTarget.Name);
						}
					} break;
				}
			}

			foreach (var entity in _state.Obstacles)
			{
				var count = _state.Entities.Count(c => c.ActionTarget == entity && c.State == Entity.States.Acting);
				SetDebugText(entity.Component, $"{entity.State}\n{count}/{entity.RequiredUnits}");

				switch (entity.State)
				{
					case Entity.States.Idle:
					{
						if (count >= entity.RequiredUnits)
						{
							entity.State = Entity.States.Moving;
						}
					} break;

					case Entity.States.Moving:
					{
						MoveTowards(entity, entity.ObstacleDestination, entity.Progress / entity.Duration);
						entity.Progress += Time.deltaTime;

						if (entity.Progress > entity.Duration)
						{
							entity.State = Entity.States.Inactive;
						}
					} break;
				}
			}

			if (_state.IsSelectionInProgress)
			{
				var mousePosition = _controls.Gameplay.MousePosition.ReadValue<Vector2>();
				var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);
				_state.SelectionEnd = mouseWorldPosition;

				_ui.SetSelectionRectangle(_state.SelectionStart, _state.SelectionEnd);
			}
			else
			{
				_ui.ClearSelectionRectangle();
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

		private void OnConfirmPressed(InputAction.CallbackContext obj)
		{
			var mousePosition = _controls.Gameplay.MousePosition.ReadValue<Vector2>();
			var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);

			_state.IsSelectionInProgress = true;
			_state.SelectionStart = mouseWorldPosition;
		}

		private void OnConfirmReleased(InputAction.CallbackContext obj)
		{
			foreach (var character in _state.SelectedUnits)
			{
				SelectCharacter(character.Component, false);
			}

			_state.SelectedUnits = new List<Entity>();
			_state.IsSelectionInProgress = false;

			var (origin, size) = GetSelectionBox(_state.SelectionStart, _state.SelectionEnd);
			var hits = Physics2D.BoxCastAll(origin, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)), 0f, Vector2.zero);
			foreach (var hit in hits)
			{
				var entityComponent = hit.transform.GetComponentInParent<CharacterComponent>();
				var entity = GetEntity(entityComponent);
				if (entity?.Type == Entity.Types.Unit)
				{
					_state.SelectedUnits.Add(entity);
					SelectCharacter(entityComponent, true);
				}
			}

			_ui.SelectSelectedCharacters(_state.SelectedUnits);
		}

		private void OnCancelReleased(InputAction.CallbackContext obj)
		{
			var mousePosition = _controls.Gameplay.MousePosition.ReadValue<Vector2>();
			var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);
			mouseWorldPosition.z = 0f;

			foreach (var character in _state.SelectedUnits)
			{
				OrderAtPosition(character, mouseWorldPosition);
			}
		}

		private void OrderAtPosition(Entity entity, Vector3 destination)
		{
			if (Vector3.Distance(entity.Component.RootTransform.position, destination) > MIN_MOVE_DISTANCE)
			{
				entity.State = Entity.States.Moving;
				entity.MoveDestination = destination;
			}

			var hit = Physics2D.CircleCast(destination, 0.5f, Vector2.zero);
			if (hit.collider)
			{
				var targetComponent = hit.transform.GetComponentInParent<CharacterComponent>();
				var targetCharacter = GetEntity(targetComponent);
				if (targetCharacter?.Type == Entity.Types.Obstacle)
				{
					entity.ActionTarget = targetCharacter;
				}
			}
			else
			{
				entity.ActionTarget = null;
			}
		}

		private Entity GetEntity(CharacterComponent component)
		{
			return _state.Entities.Find(character => character.Component == component);
		}
	}
}

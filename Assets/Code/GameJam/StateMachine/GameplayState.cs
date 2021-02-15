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

		private const float MIN_MOVE_DISTANCE = 1.5f;

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.Entities = new List<Entity>();
			_state.SelectedUnits = new List<Entity>();

			_astar.Scan(_astar.graphs);

			var character1 = SpawnUnit(_config.UnitPrefab, "Ariette", new Vector3(0f, 0f, 0f));
			_state.Entities.Add(character1);
			var character2 = SpawnUnit(_config.UnitPrefab, "Joe", new Vector3(3f, 2f, 0f));
			_state.Entities.Add(character2);
			var character3 = SpawnUnit(_config.UnitPrefab, "Jessi", new Vector3(-5f, -2f, 0f));
			_state.Entities.Add(character3);

			var obstacle1 = SpawnObstacle(_config.ObstaclePrefab, "Obstacle1", new Vector3(-26f, 6f, 0f), 2, 2, new Vector3(-26f, 0f, 0f));
			_state.Entities.Add(obstacle1);
			var obstacle2 = SpawnObstacle(_config.ObstaclePrefab, "Obstacle2", new Vector3(-2f, 22f, 0f), 1, 2, new Vector3(2f, 22f, 0f));
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

			var moveInput = _controls.Gameplay.Move.ReadValue<Vector2>();
			if (moveInput.magnitude > 0f)
			{
				var cameraMoveSpeed = 10f;
				_camera.transform.position = Vector3.Lerp(_camera.transform.position, _camera.transform.position + new Vector3(moveInput.x, moveInput.y, 0f), cameraMoveSpeed * Time.deltaTime);
			}

			foreach (var entity in _state.Units)
			{
				SetDebugText(entity.Component, $"{entity.State}");

				switch (entity.State)
				{
					case Entity.States.Idle:
					{
						entity.Component.AI.canMove = false;
						entity.Component.Rigidbody.velocity = Vector3.zero;
					} break;

					case Entity.States.Moving:
					{
						entity.Component.AI.canMove = true;
						entity.Component.AI.destination = entity.MoveDestination;

						if (Vector3.Distance(entity.Component.RootTransform.position, entity.MoveDestination) < MIN_MOVE_DISTANCE)
						{
							if (entity.ActionTarget == null)
							{
								entity.State = Entity.States.Idle;
							}
							else
							{
								entity.State = Entity.States.Acting;
							}
						}
					} break;

					case Entity.States.Acting:
					{
						if (entity.ActionTarget.State == Entity.States.Inactive)
						{
							entity.State = Entity.States.Idle;
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
						if (count >= entity.RequiredUnits)
						{
							entity.Progress += Time.deltaTime;

							if (entity.Progress > entity.Duration)
							{
								entity.Component.RootTransform.position = entity.ObstacleDestination;
								_astar.UpdateGraphs(new Bounds(entity.Component.RootTransform.position, new Vector3Int(10, 10, 1)));
								entity.State = Entity.States.Inactive;
							}
						}
						else
						{
							entity.State = Entity.States.Idle;
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
			var hits = Physics2D.BoxCastAll(origin, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)), 0f, Vector2.zero, 0f, _config.SelectionMask);
			foreach (var hit in hits)
			{
				var entityComponent = hit.transform.GetComponentInParent<EntityComponent>();
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

			foreach (var entity in _state.SelectedUnits)
			{
				OrderAtPosition(entity, mouseWorldPosition);
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
				var targetComponent = hit.transform.GetComponentInParent<EntityComponent>();
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

		private Entity GetEntity(EntityComponent component)
		{
			return _state.Entities.Find(character => character.Component == component);
		}
	}
}

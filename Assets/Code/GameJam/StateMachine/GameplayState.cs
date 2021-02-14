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

			_state.AllCharacters = new List<Character>();
			_state.SelectedCharacters = new List<Character>();

			var character1 = SpawnUnit(_config.UnitPrefab, "Ariette", new Vector3(0f, 0f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character1);
			var character2 = SpawnUnit(_config.UnitPrefab, "Joe", new Vector3(3f, 2f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character2);
			var character3 = SpawnUnit(_config.UnitPrefab, "Jessi", new Vector3(-5f, -2f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character3);

			var obstacle1 = SpawnCharacter(_config.ObstaclePrefab, "Obstacle1", new Vector3(9f, 7f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(obstacle1);
			var obstacle2 = SpawnCharacter(_config.ObstaclePrefab, "Obstacle2", new Vector3(5f, -3f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(obstacle2);

			foreach (var character in _state.AllCharacters)
			{
				SelectCharacter(character.Component, false);
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

			foreach (var character in _state.AllCharacters)
			{
				// TODO: optimize this
				ShowCounter(character.Component, _state.AllCharacters.Count(
					c => c.ActionTarget == character && Vector3.Distance(c.Component.RootTransform.position, c.MoveDestination) < MIN_MOVE_DISTANCE)
				);

				if (character.NeedsToMove)
				{
					MoveTo(character, character.MoveDestination);

					if (Vector3.Distance(character.Component.RootTransform.position, character.MoveDestination) < MIN_MOVE_DISTANCE)
					{
						character.NeedsToMove = false;
					}
				}

				if (character.NeedsToMove == false && character.ActionTarget != null)
				{
					// Debug.Log(character.Name + " interacting with " + character.ActionTarget.Name);
				}
			}

			if (_state.SelectionInProgress)
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

			_state.SelectionInProgress = true;
			_state.SelectionStart = mouseWorldPosition;
		}

		private void OnConfirmReleased(InputAction.CallbackContext obj)
		{
			foreach (var character in _state.SelectedCharacters)
			{
				SelectCharacter(character.Component, false);
			}

			_state.SelectedCharacters = new List<Character>();
			_state.SelectionInProgress = false;

			var (origin, size) = GetSelectionBox(_state.SelectionStart, _state.SelectionEnd);
			var hits = Physics2D.BoxCastAll(origin, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)), 0f, Vector2.zero);
			foreach (var hit in hits)
			{
				var component = hit.transform.GetComponentInParent<CharacterComponent>();
				var character = GetCharacter(component);
				if (character?.IsUnit == true)
				{
					_state.SelectedCharacters.Add(character);
					SelectCharacter(component, true);
				}
			}

			_ui.SelectSelectedCharacters(_state.SelectedCharacters);
		}

		private void OnCancelReleased(InputAction.CallbackContext obj)
		{
			var mousePosition = _controls.Gameplay.MousePosition.ReadValue<Vector2>();
			var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);
			mouseWorldPosition.z = 0f;

			foreach (var character in _state.SelectedCharacters)
			{
				OrderAtPosition(character, mouseWorldPosition);
			}
		}

		private void OrderAtPosition(Character character, Vector3 destination)
		{
			if (Vector3.Distance(character.Component.RootTransform.position, destination) > MIN_MOVE_DISTANCE)
			{
				character.NeedsToMove = true;
				character.MoveDestination = destination;
			}

			var hit = Physics2D.CircleCast(destination, 0.5f, Vector2.zero);
			if (hit.collider)
			{
				var targetComponent = hit.transform.GetComponentInParent<CharacterComponent>();
				var targetCharacter = GetCharacter(targetComponent);
				if (targetCharacter?.IsUnit == false)
				{
					character.ActionTarget = targetCharacter;
				}
			}
			else
			{
				character.ActionTarget = null;
			}
		}

		private Character GetCharacter(CharacterComponent component)
		{
			return _state.AllCharacters.Find(character => character.Component == component);
		}
	}
}

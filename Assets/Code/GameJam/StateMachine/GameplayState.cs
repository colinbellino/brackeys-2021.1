using System.Collections.Generic;
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

			_state.AllCharacters = new List<Character>();
			_state.SelectedCharacters = new List<Character>();
			var character1 = SpawnCharacter(_config.UnitPrefab, "Ariette", new Vector3(0f, 0f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character1);
			var character2 = SpawnCharacter(_config.UnitPrefab, "Joe", new Vector3(3f, 2f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character2);
			var character3 = SpawnCharacter(_config.UnitPrefab, "Jessi", new Vector3(-5f, -2f, 0f), Quaternion.identity);
			_state.AllCharacters.Add(character3);

			foreach (var character in _state.AllCharacters)
			{
				character.Component.Selection.SetActive(false);
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
				if (character.NeedsToMove)
				{
					var motion = (character.MoveDestination - character.Component.RootTransform.position).normalized;
					character.Component.CharacterController.Move(motion * (character.MoveSpeed * Time.deltaTime));
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
				character.Component.Selection.SetActive(false);
			}

			_state.SelectedCharacters = new List<Character>();
			_state.SelectionInProgress = false;

			var (origin, size) = GetSelectionBox(_state.SelectionStart, _state.SelectionEnd);
			var hits = Physics2D.BoxCastAll(origin, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)), 0f, Vector2.zero);
			foreach (var hit in hits)
			{
				var character = hit.transform.GetComponentInParent<CharacterComponent>();
				if (character != null)
				{
					_state.SelectedCharacters.Add(GetCharacter(character));
					character.Selection.SetActive(true);
				}
			}

			_ui.SelectSelectedCharacters(_state.SelectedCharacters);
		}

		private void OnCancelReleased(InputAction.CallbackContext obj)
		{
			foreach (var character in _state.SelectedCharacters)
			{
				SetOrder(character, 1);
			}
		}

		private void SetOrder(Character character, int order)
		{
			switch (order)
			{
				case 1:
				{
					var mousePosition = _controls.Gameplay.MousePosition.ReadValue<Vector2>();
					var mouseWorldPosition = _camera.ScreenToWorldPoint(mousePosition);
					mouseWorldPosition.z = 0f;

					Debug.Log(character.Name + " move to " + mouseWorldPosition);
					character.NeedsToMove = true;
					character.MoveDestination = mouseWorldPosition;
				}
				break;
				default:
				{
					Debug.LogError("Unknown order: " + order);
				}
				break;
			}
		}

		private Character GetCharacter(CharacterComponent component)
		{
			return _state.AllCharacters.Find(character => character.Component == component);
		}
	}
}

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

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.ConfirmPress.performed += OnConfirmPressed;
			_controls.Gameplay.Confirm.performed += OnConfirmReleased;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.ConfirmPress.performed -= OnConfirmPressed;
			_controls.Gameplay.Confirm.performed -= OnConfirmReleased;
		}

		public override void Tick()
		{
			base.Tick();

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
			_state.SelectedCharacters = new List<Character>();
			_state.SelectionInProgress = false;

			var (origin, size) = GetBox(_state.SelectionStart, _state.SelectionEnd);
			var hits = Physics2D.BoxCastAll(origin, new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y)), 0f, Vector2.zero);
			foreach (var hit in hits)
			{
				var character = hit.transform.GetComponentInParent<CharacterComponent>();
				if (character != null)
				{
					Debug.Log("hit : " + character.name);
					_state.SelectedCharacters.Add(GetCharacter(character));
				}
			}

			_ui.SelectSelectedCharacters(_state.SelectedCharacters);
		}

		private Character GetCharacter(CharacterComponent component)
		{
			return _state.AllCharacters.Find(character => character.Component == component);
		}
	}
}

using Cysharp.Threading.Tasks;
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

			_ui.ShowGameplay();

			_controls.Gameplay.Enable();
			_controls.Gameplay.Confirm.performed += OnConfirmPerformed;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideGameplay();

			_controls.Gameplay.Disable();
			_controls.Gameplay.Confirm.performed -= OnConfirmPerformed;
		}

		public override void Tick()
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
			}
		}

		private void OnConfirmPerformed(InputAction.CallbackContext obj)
		{

		}
	}
}

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameJam
{
	public class DefeatState : BaseGameState
	{
		private List<string> _helpers;

		public DefeatState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_state.DeathCounter += 1;

			_helpers = await Utils.LoadHelpers();

			_ui.ShowGiveUp();
			_ui.GiveUpYesButton.onClick.AddListener(GiveUp);
			_ui.GiveUpNoButton.onClick.AddListener(Continue);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.GiveUpYesButton.onClick.RemoveListener(GiveUp);
			_ui.GiveUpNoButton.onClick.RemoveListener(Continue);
			_ui.ReceiveHelpYesButton.onClick.RemoveListener(RestartWithHelp);
			_ui.ReceiveHelpNoButton.onClick.RemoveListener(RestartWithoutHelp);
		}

		private void GiveUp()
		{
			_machine.Fire(GameStateMachine.Triggers.Quit);
		}

		private void Continue()
		{
			_ui.HideGiveUp();
			if (_state.DeathCounter < 3)
			{
				RestartWithoutHelp();
				return;
			}

			_ui.ShowReceiveHelp(_helpers[0]);
			_ui.ReceiveHelpYesButton.onClick.AddListener(RestartWithHelp);
			_ui.ReceiveHelpNoButton.onClick.AddListener(RestartWithoutHelp);
		}

		private async void RestartWithHelp()
		{
			Debug.Log("Accepted help.");
			_state.HelpersName = _helpers;
			_ui.HideReceiveHelp();

			await _ui.EndFadeToBlack();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private async void RestartWithoutHelp()
		{
			_ui.HideReceiveHelp();

			await _ui.EndFadeToBlack();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

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

			_ = _ui.ShowGiveUp(_state.DeathCounter);
			_ui.GiveUpYesButton.onClick.AddListener(GiveUp);
			_ui.GiveUpNoButton.onClick.AddListener(Continue);
			_ui.ReceiveHelpYesButton.onClick.AddListener(RestartWithHelp);
			_ui.ReceiveHelpNoButton.onClick.AddListener(RestartWithoutHelp);

			_state.DeathCounter += 1;
			_ui.SetCounter(_state.DeathCounter);
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

		private async void Continue()
		{
			await _ui.HideGiveUp();
			if (_state.DeathCounter < 3 || _state.HelpReceived)
			{
				RestartWithoutHelp();
				return;
			}

			_helpers = await Utils.LoadHelpers();
			_ = _ui.ShowReceiveHelp(_helpers[Random.Range(0, _helpers.Count)]);
		}

		private async void RestartWithHelp()
		{
			await _ui.HideReceiveHelp();
			await _audioPlayer.StopMusic();

			_state.HelpersName = _helpers;
			_state.HelpReceived = true;

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private async void RestartWithoutHelp()
		{
			await _ui.HideReceiveHelp();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

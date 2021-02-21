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

			_ui.ShowGiveUp(_state.DeathCounter);
			_ui.GiveUpYesButton.onClick.AddListener(GiveUp);
			_ui.GiveUpNoButton.onClick.AddListener(Continue);

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
			_ui.HideGiveUp();
			if (_state.DeathCounter < 3)
			{
				RestartWithoutHelp();
				return;
			}

			_helpers = await Utils.LoadHelpers();
			_ui.ShowReceiveHelp(_helpers[Random.Range(0, _helpers.Count)]);
			_ui.ReceiveHelpYesButton.onClick.AddListener(RestartWithHelp);
			_ui.ReceiveHelpNoButton.onClick.AddListener(RestartWithoutHelp);
		}

		private void RestartWithHelp()
		{
			_ui.HideReceiveHelp();

			_ = _audioPlayer.StopMusic(1f);

			_state.HelpersName = _helpers;
			_state.HelpReceived = true;

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private void RestartWithoutHelp()
		{
			_ui.HideReceiveHelp();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

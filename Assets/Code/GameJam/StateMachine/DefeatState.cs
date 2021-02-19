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

			_helpers = await Utils.LoadHelpers();

			_ui.ShowGiveUp();
			_ui.GiveUpYesButton.onClick.AddListener(GiveUpYesClicked);
			_ui.GiveUpNoButton.onClick.AddListener(GiveUpNoClicked);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.GiveUpYesButton.onClick.RemoveListener(GiveUpYesClicked);
			_ui.GiveUpNoButton.onClick.RemoveListener(GiveUpNoClicked);
		}

		private void GiveUpYesClicked()
		{
			_machine.Fire(GameStateMachine.Triggers.Quit);
		}

		private void GiveUpNoClicked()
		{
			_ui.HideGiveUp();
			_ui.ShowReceiveHelp(_helpers[0]);
			_ui.ReceiveHelpYesButton.onClick.AddListener(ReceiveHelpYesClicked);
			_ui.ReceiveHelpNoButton.onClick.AddListener(ReceiveHelpNoClicked);
		}

		private async void ReceiveHelpYesClicked()
		{
			Debug.Log("Accepted help.");
			_state.Helpers = _helpers;
			_ui.HideReceiveHelp();

			await _ui.EndFadeToBlack();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private async void ReceiveHelpNoClicked()
		{
			_ui.HideReceiveHelp();

			await _ui.EndFadeToBlack();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

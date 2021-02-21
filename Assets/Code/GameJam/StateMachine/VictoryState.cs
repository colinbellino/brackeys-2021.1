using Cysharp.Threading.Tasks;

namespace GameJam
{
	public class VictoryState : BaseGameState
	{
		public VictoryState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.ShowVictory(_state.HelpReceived);
			_ui.VictoryNextButton.onClick.AddListener(VictoryNextClicked);
			_ui.RetryYesButton.onClick.AddListener(RetryYesClicked);
			_ui.RetryNoButton.onClick.AddListener(RetryNoClicked);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.VictoryNextButton.onClick.RemoveListener(VictoryNextClicked);
			_ui.RetryYesButton.onClick.RemoveListener(RetryYesClicked);
			_ui.RetryNoButton.onClick.RemoveListener(RetryNoClicked);
		}

		private void VictoryNextClicked()
		{
			_ui.HideVictory();
			_ui.ShowRetry();
		}

		private async void RetryYesClicked()
		{
			_ui.HideRetry();

			await _ui.FadeOut();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private void RetryNoClicked()
		{
			_machine.Fire(GameStateMachine.Triggers.Quit);
		}
	}
}

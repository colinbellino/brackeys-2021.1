using Cysharp.Threading.Tasks;

namespace GameJam
{
	public class VictoryState : BaseGameState
	{
		public VictoryState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			await _ui.ShowTitle();
			await UniTask.Delay(1000);

			await _ui.ShowVictory(_state.HelpReceived);
			_ui.VictoryNextButton.onClick.AddListener(VictoryNextClicked);
			_ui.CommentNextButton.onClick.AddListener(CommentNextClicked);
			_ui.RetryYesButton.onClick.AddListener(RetryYesClicked);
			_ui.RetryNoButton.onClick.AddListener(RetryNoClicked);

			_ = _audioPlayer.StopMusic(5f);
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.VictoryNextButton.onClick.RemoveListener(VictoryNextClicked);
			_ui.CommentNextButton.onClick.RemoveListener(CommentNextClicked);
			_ui.RetryYesButton.onClick.RemoveListener(RetryYesClicked);
			_ui.RetryNoButton.onClick.RemoveListener(RetryNoClicked);
		}

		private async void VictoryNextClicked()
		{
			await _ui.HideVictory();
			await _ui.ShowComment();
		}

		private async void CommentNextClicked()
		{
			await _ui.HideComment();
			await _ui.ShowRetry();
		}

		private void RetryYesClicked()
		{
			_ = _ui.HideRetry();
			_ = _ui.HideTitle();

			_machine.Fire(GameStateMachine.Triggers.Retry);
		}

		private void RetryNoClicked()
		{
			_machine.Fire(GameStateMachine.Triggers.Quit);
		}
	}
}

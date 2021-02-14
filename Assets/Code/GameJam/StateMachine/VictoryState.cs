using Cysharp.Threading.Tasks;

namespace GameJam
{
	public class VictoryState : BaseGameState
	{
		public VictoryState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.ShowVictory();
			_ui.RetryClicked += OnRetryClicked;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideVictory();
			_ui.RetryClicked -= OnRetryClicked;
		}

		private void OnRetryClicked()
		{
			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

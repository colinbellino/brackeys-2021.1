using Cysharp.Threading.Tasks;

namespace GameJam
{
	public class DefeatState : BaseGameState
	{
		public DefeatState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			_ui.ShowDefeat();
			_ui.RetryClicked += OnRetryClicked;
		}

		public override async UniTask Exit()
		{
			await base.Exit();

			_ui.HideDefeat();
			_ui.RetryClicked -= OnRetryClicked;
		}

		private void OnRetryClicked()
		{
			_machine.Fire(GameStateMachine.Triggers.Retry);
		}
	}
}

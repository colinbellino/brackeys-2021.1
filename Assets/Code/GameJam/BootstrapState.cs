using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameJam
{
	public class BootstrapState : BaseGameState
	{
		public BootstrapState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

			// Game.Instance.AudioPlayer.SetMusicVolume(Game.Instance.Config.MusicVolume);
			// Game.Instance.AudioPlayer.SetSoundVolume(Game.Instance.Config.SoundVolume);

			Time.timeScale = 1f;

			_machine.Fire(GameStateMachine.Triggers.Done);
		}
	}
}

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

			_projectileSpawner.CreatePool(200, _config.DefaultProjectilePrefab);

			Time.timeScale = 1f;

			await UniTask.NextFrame();

			_machine.Fire(GameStateMachine.Triggers.Done);
		}
	}
}

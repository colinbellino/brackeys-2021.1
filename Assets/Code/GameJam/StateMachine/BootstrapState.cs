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

			_audioPlayer.SetMusicVolume(_config.MusicVolume);
			_audioPlayer.SetSoundVolume(_config.SoundVolume);

			_projectileSpawner.CreatePool(1000, _config.DefaultProjectilePrefab);

			Time.timeScale = 1f;

			await UniTask.NextFrame();

			_machine.Fire(GameStateMachine.Triggers.Done);
		}
	}
}

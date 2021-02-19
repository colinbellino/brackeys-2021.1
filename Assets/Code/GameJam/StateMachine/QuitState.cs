using Cysharp.Threading.Tasks;
using UnityEditor;

namespace GameJam
{
	public class QuitState : BaseGameState
	{
		public QuitState(GameStateMachine machine, Game game) : base(machine, game) { }

		public override async UniTask Enter()
		{
			await base.Enter();

#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			UnityEngine.Application.Quit();
#endif
		}
	}
}

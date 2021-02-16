using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameJam
{
	public abstract class BaseGameState : IState
	{
		protected readonly GameStateMachine _machine;
		protected readonly Game _game;

		protected GameConfig _config => _game.Config;
		protected GameUI _ui => _game.UI;
		protected Camera _camera => _game.Camera;
		protected GameControls _controls => _game.Controls;
		protected GameState _state => _game.State;

		protected BaseGameState(GameStateMachine machine, Game game)
		{
			_machine = machine;
			_game = game;
		}

		public virtual UniTask Enter() { return default; }

		public virtual UniTask Exit() { return default; }

		public virtual void Tick() { }
	}
}

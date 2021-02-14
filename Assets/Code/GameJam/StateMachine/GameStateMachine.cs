using System.Collections.Generic;
using Stateless;

namespace GameJam
{
	public class GameStateMachine
	{
		public enum States
		{
			Bootstrap,
			Gameplay,
			Victory,
			Defeat,
		}
		public enum Triggers
		{
			Done,
			Victory,
			Defeat,
			Retry,
			Quit,
		}

		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;
		private bool _debug;

		public GameStateMachine(bool debug, Game game)
		{
			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Bootstrap, new BootstrapState(this, game) },
				{ States.Gameplay, new GameplayState(this, game) },
				{ States.Victory, new VictoryState(this, game) },
				{ States.Defeat, new DefeatState(this, game) },
			};

			_machine = new StateMachine<States, Triggers>(States.Bootstrap);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Bootstrap)
				.Permit(Triggers.Done, States.Gameplay);

			_machine.Configure(States.Gameplay)
				.Permit(Triggers.Victory, States.Victory)
				.Permit(Triggers.Defeat, States.Defeat);

			_machine.Configure(States.Victory)
				.Permit(Triggers.Retry, States.Gameplay);

			_machine.Configure(States.Defeat)
				.Permit(Triggers.Retry, States.Gameplay);

			_currentState = _states[_machine.State];
		}

		public async void Start()
		{
			await _currentState.Enter();
		}

		public void Tick() => _currentState?.Tick();

		public void Fire(Triggers trigger) => _machine.Fire(trigger);

		private async void OnTransitioned(StateMachine<States, Triggers>.Transition transition)
		{
			if (_currentState != null)
			{
				await _currentState.Exit();
			}

			if (_debug)
			{
				if (_states.ContainsKey(transition.Destination) == false)
				{
					UnityEngine.Debug.LogError("Missing state class for: " + transition.Destination);
				}
			}

			_currentState = _states[transition.Destination];
			if (_debug)
			{
				UnityEngine.Debug.Log($"{transition.Source} -> {transition.Destination}");
			}

			await _currentState.Enter();
		}
	}
}

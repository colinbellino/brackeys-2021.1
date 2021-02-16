using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;

namespace GameJam
{
	public class ObstacleStateMachine
	{
		public enum States { Idle, Moving, Inactive }
		public enum Triggers { StartMoving, StopMoving, Done }

		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;
		private readonly bool _debug;

		public ObstacleStateMachine(bool debug, Game game, Obstacle entity)
		{
			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Idle, new IdleState(this, game, entity) },
				{ States.Moving, new MovingState(this, game, entity) },
				{ States.Inactive, new InactiveState(this, game, entity) },
			};

			_machine = new StateMachine<States, Triggers>(States.Idle);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Idle)
				.Permit(Triggers.StartMoving, States.Moving);
			_machine.Configure(States.Moving)
				.Permit(Triggers.StopMoving, States.Idle)
				.Permit(Triggers.Done, States.Inactive);

			_currentState = _states[_machine.State];
		}

		public UniTask Start() => _currentState.Enter();

		public void Tick() => _currentState?.Tick();

		public void Fire(Triggers trigger) => _machine.Fire(trigger);

		public bool CanFire(Triggers trigger) => _machine.CanFire(trigger);

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

		private class BaseObstacleState : IState
		{
			protected readonly ObstacleStateMachine _machine;
			protected readonly Game _game;
			protected readonly Obstacle _actor;

			protected BaseObstacleState(ObstacleStateMachine machine, Game game, Obstacle actor)
			{
				_machine = machine;
				_game = game;
				_actor = actor;
			}

			public virtual UniTask Enter() { return default; }

			public virtual UniTask Exit() { return default; }

			public virtual void Tick()
			{
				Utils.SetDebugText(_actor.Component, $"{GetType().Name}");
			}
		}

		private class IdleState : BaseObstacleState
		{
			public IdleState(ObstacleStateMachine machine, Game game, Obstacle actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				if (_actor.PushedBy.Count >= _actor.RequiredUnits)
				{
					_machine.Fire(Triggers.StartMoving);
				}
			}
		}

		private class MovingState : BaseObstacleState
		{
			public MovingState(ObstacleStateMachine machine, Game game, Obstacle actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				if (_actor.PushedBy.Count >= _actor.RequiredUnits)
				{
					_actor.Progress += Time.deltaTime;

					if (_actor.Progress > _actor.Duration)
					{
						_actor.Component.RootTransform.position = _actor.PushDestination;
						_game.Astar.UpdateGraphs(new Bounds(_actor.Component.RootTransform.position, new Vector3Int(10, 10, 1)));
						_machine.Fire(Triggers.Done);
					}
				}
				else
				{
					_machine.Fire(Triggers.StopMoving);
				}
			}
		}

		private class InactiveState : BaseObstacleState
		{
			public InactiveState(ObstacleStateMachine machine, Game game, Obstacle actor) : base(machine, game, actor) { }
		}
	}
}

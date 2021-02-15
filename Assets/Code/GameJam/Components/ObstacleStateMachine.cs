using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;

namespace GameJam
{
	public class ObstacleStateMachine
	{
		public enum States { Idle, Moving }
		public enum Triggers { StartMoving, Done }

		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;
		private readonly bool _debug;

		public ObstacleStateMachine(bool debug, Game game, Entity entity)
		{
			_debug = debug;
			_states = new Dictionary<States, IState>
			{
				{ States.Idle, new IdleState(this, game, entity) },
				{ States.Moving, new MovingState(this, game, entity) },
			};

			_machine = new StateMachine<States, Triggers>(States.Idle);
			_machine.OnTransitioned(OnTransitioned);

			_machine.Configure(States.Idle)
				.Permit(Triggers.StartMoving, States.Moving)
				.Permit(Triggers.StartPushing, States.Pushing);
			_machine.Configure(States.Moving)
				.Permit(Triggers.StartPushing, States.Pushing)
				.Permit(Triggers.Done, States.Idle);
			_machine.Configure(States.Pushing)
				.Permit(Triggers.StartMoving, States.Moving)
				.Permit(Triggers.Done, States.Idle);

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
			protected readonly Entity _entity;

			protected BaseObstacleState(ObstacleStateMachine machine, Game game, Entity entity)
			{
				_machine = machine;
				_game = game;
				_entity = entity;
			}

			public virtual UniTask Enter() { return default; }

			public virtual UniTask Exit() { return default; }

			public virtual void Tick()
			{
				Utils.SetDebugText(_entity.Component, $"{GetType().Name}");
			}
		}

		private class BootstrapEntity : BaseObstacleState
		{
			public BootstrapEntity(ObstacleStateMachine machine, Game game, Entity entity) : base(machine, game, entity) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_machine.Fire(Triggers.Done);
			}
		}

		private class IdleState : BaseObstacleState
		{
			public IdleState(ObstacleStateMachine machine, Game game, Entity entity) : base(machine, game, entity) { }

			public override void Tick()
			{
				base.Tick();

				_entity.Component.AI.canMove = false;
				_entity.Component.Rigidbody.velocity = Vector3.zero;
			}
		}

		private class MovingState : BaseObstacleState
		{
			public MovingState(ObstacleStateMachine machine, Game game, Entity entity) : base(machine, game, entity) { }

			public override void Tick()
			{
				base.Tick();

				_entity.Component.AI.canMove = true;
				_entity.Component.AI.destination = _entity.MoveDestination;

				if (Vector3.Distance(_entity.Component.RootTransform.position, _entity.MoveDestination) < Entity.MIN_MOVE_DISTANCE)
				{
					if (_entity.ActionTarget == null)
					{
						_machine.Fire(Triggers.Done);
					}
					else
					{
						_machine.Fire(Triggers.StartPushing);
					}
				}
			}
		}

		private class PushingState : BaseObstacleState
		{
			public PushingState(ObstacleStateMachine machine, Game game, Entity entity) : base(machine, game, entity) { }

			public override void Tick()
			{
				base.Tick();

				if (_entity.ActionTarget?.Progress >= _entity.ActionTarget?.Duration)
				{
					_machine.Fire(Triggers.Done);
				}
			}
		}
	}
}

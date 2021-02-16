using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;
using static GameJam.Utils;

namespace GameJam
{
	public class UnitStateMachine
	{
		public enum States { Inactive, Idle, Following, Moving, Pushing }
		public enum Triggers { Thrown, StartFollowing, StartPushing, Done }

		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;
		private readonly bool _debug;
		private Unit _entity;

		public UnitStateMachine(bool debug, Game game, Unit entity)
		{
			_debug = debug;
			_entity = entity;
			_states = new Dictionary<States, IState>
			{
				{ States.Inactive, new InactiveState(this, game, entity) },
				{ States.Idle, new IdleState(this, game, entity) },
				{ States.Following, new FollowingState(this, game, entity) },
				{ States.Moving, new ThrownState(this, game, entity) },
				{ States.Pushing, new PushingState(this, game, entity) },
			};

			_machine = new StateMachine<States, Triggers>(States.Inactive);

			_machine.Configure(States.Inactive)
				.Permit(Triggers.StartFollowing, States.Following);

			_machine.Configure(States.Idle)
				.Permit(Triggers.StartFollowing, States.Following)
				.Permit(Triggers.StartPushing, States.Pushing);

			_machine.Configure(States.Following)
				.Permit(Triggers.Thrown, States.Moving);

			_machine.Configure(States.Moving)
				.Permit(Triggers.Done, States.Idle);

			_machine.Configure(States.Pushing)
				.Permit(Triggers.StartFollowing, States.Following)
				.Permit(Triggers.Done, States.Idle);

			_machine.OnTransitioned(OnTransitioned);

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
				UnityEngine.Debug.Log($"{_entity}: {transition.Source} -> {transition.Destination}");
			}

			await _currentState.Enter();
		}

		private class BaseEntityState : IState
		{
			protected readonly UnitStateMachine _machine;
			protected readonly Game _game;
			protected readonly Unit _actor;

			protected BaseEntityState(UnitStateMachine machine, Game game, Unit actor)
			{
				_machine = machine;
				_game = game;
				_actor = actor;
			}

			public virtual UniTask Enter() { return default; }

			public virtual UniTask Exit() { return default; }

			public virtual void Tick()
			{
				SetDebugText(_actor.Component, $"{GetType().Name}");
			}
		}

		private class InactiveState : BaseEntityState
		{
			public InactiveState(UnitStateMachine machine, Game game, Unit actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				TryFollowLeader(_actor, _game);
			}
		}

		private class IdleState : BaseEntityState
		{
			public IdleState(UnitStateMachine machine, Game game, Unit actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				_actor.Component.Rigidbody.velocity = Vector3.zero;

				TryPushObstacles(_actor, _game);
				TryFollowLeader(_actor, _game);
			}
		}

		private class FollowingState : BaseEntityState
		{
			public FollowingState(UnitStateMachine machine, Game game, Unit actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				var difference = _actor.FollowTarget.Component.RootTransform.position - _actor.Component.RootTransform.transform.position;
				if (difference.magnitude > Entity.MIN_FOLLOW_DISTANCE)
				{
					_actor.Component.RootTransform.transform.position = Vector3.Lerp(
						_actor.Component.RootTransform.transform.position,
						_actor.Component.RootTransform.transform.position + difference.normalized,
						Time.deltaTime * _actor.MoveSpeed
					);
				}
			}
		}

		private class ThrownState : BaseEntityState
		{
			public ThrownState(UnitStateMachine machine, Game game, Unit actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				var difference = _actor.MoveDestination - _actor.Component.RootTransform.transform.position;
				_actor.Component.Rigidbody.MovePosition(_actor.Component.RootTransform.transform.position + difference.normalized * (Time.fixedDeltaTime * _actor.ThrowSpeed));

				// TODO: stop if we hit a wall

				if (Vector3.Distance(_actor.Component.RootTransform.position, _actor.MoveDestination) <= 0.1f)
				{
					_machine.Fire(Triggers.Done);
				}
			}
		}

		private class PushingState : BaseEntityState
		{
			public PushingState(UnitStateMachine machine, Game game, Unit actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_actor.ActionTarget.PushedBy.Add(_actor);
			}

			public override async UniTask Exit()
			{
				await base.Exit();

				_actor.ActionTarget.PushedBy.Remove(_actor);
				_actor.ActionTarget = null;
			}

			public override void Tick()
			{
				base.Tick();

				TryFollowLeader(_actor, _game);

				if (_actor.ActionTarget?.Progress >= _actor.ActionTarget?.Duration)
				{
					_machine.Fire(Triggers.Done);
				}
			}
		}
	}
}

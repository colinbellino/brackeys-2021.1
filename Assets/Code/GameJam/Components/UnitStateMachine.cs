using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Stateless;
using UnityEngine;
using static GameJam.Utils;

namespace GameJam
{
	public class UnitStateMachine
	{
		public enum States { Inactive, IdleShooter, PlayerControlState, Destroy }
		public enum Triggers { Done, Destroyed, ControlledByPlayer }

		private readonly Dictionary<States, IState> _states;
		private readonly StateMachine<States, Triggers> _machine;
		private IState _currentState;
		private readonly bool _debug;
		private EntityComponent _actor;

		public UnitStateMachine(bool debug, Game game, EntityComponent actor)
		{
			_debug = debug;
			_actor = actor;
			_states = new Dictionary<States, IState>
			{
				{ States.Inactive, new InactiveState(this, game, actor) },
				{ States.PlayerControlState, new PlayerControlState(this, game, actor) },
				{ States.IdleShooter, new IdleShooterState(this, game, actor) },
				{ States.Destroy, new DestroyState(this, game, actor) },
			};

			_machine = new StateMachine<States, Triggers>(States.Inactive);

			_machine.Configure(States.Inactive)
				.PermitDynamicIf(Triggers.Done, () => States.PlayerControlState, () => _actor.Brain == AI.None)
				.PermitDynamicIf(Triggers.Done, () => States.IdleShooter, () => _actor.Brain == AI.IdleShooter);

			_machine.Configure(States.IdleShooter)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.Configure(States.PlayerControlState)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.OnTransitioned(OnTransitioned);

			_currentState = _states[_machine.State];
		}

		public UniTask Start() => _currentState.Enter();

		public void Tick() => _currentState?.Tick();

		public void Fire(Triggers trigger)
		{
			if (_machine.CanFire(trigger))
			{
				_machine.Fire(trigger);
			}
			else
			{
				Debug.LogWarning("Invalid transition " + _currentState + " -> " + trigger);
			}
		}

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
					Debug.LogError("Missing state class for: " + transition.Destination);
				}
			}

			_currentState = _states[transition.Destination];
			if (_debug)
			{
				Debug.Log($"{_actor}: {transition.Source} -> {transition.Destination}");
			}

			await _currentState.Enter();
		}

		private class BaseEntityState : IState
		{
			protected readonly UnitStateMachine _machine;
			protected readonly Game _game;
			protected readonly EntityComponent _actor;

			protected BaseEntityState(UnitStateMachine machine, Game game, EntityComponent actor)
			{
				_machine = machine;
				_game = game;
				_actor = actor;
			}

			public virtual UniTask Enter() { return default; }

			public virtual UniTask Exit() { return default; }

			public virtual void Tick()
			{
				SetDebugText(_actor, $"{GetType().Name}");
			}
		}

		private class InactiveState : BaseEntityState
		{
			public InactiveState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_machine.Fire(Triggers.Done);
			}
		}

		private class IdleShooterState : BaseEntityState
		{
			private float _activationTimestamp;

			public IdleShooterState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_activationTimestamp = Time.time + 3f;
			}

			public override void Tick()
			{
				base.Tick();

				if (Time.time >= _activationTimestamp)
				{
					_actor.transform.Rotate(_actor.RotationPerTick * Time.deltaTime);

					_actor.Rigidbody.velocity = Vector3.zero;
					FireProjectile(_actor, _game.State);
				}
			}
		}

		private class PlayerControlState : BaseEntityState
		{
			public PlayerControlState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				var mouseWorldPosition = GetMouseWorldPosition(_game.Controls, _game.Camera);
				var moveInput = _game.Controls.Gameplay.Move.ReadValue<Vector2>();
				var confirmInput = _game.Controls.Gameplay.Confirm.ReadValue<float>();

				_actor.Rigidbody.velocity = Vector3.zero;
				_actor.transform.position = Vector3.Lerp(
					_actor.transform.position,
					_actor.transform.position + new Vector3(moveInput.x, moveInput.y, 0f),
					_actor.MoveSpeed * Time.deltaTime
				);
				_actor.transform.up = mouseWorldPosition - _actor.transform.position;

				if (confirmInput > 0f)
				{
					FireProjectile(_actor, _game.State);
				}
			}
		}

		private class DestroyState : BaseEntityState
		{
			public DestroyState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				GameObject.Destroy(_actor.gameObject);
			}
		}
	}
}

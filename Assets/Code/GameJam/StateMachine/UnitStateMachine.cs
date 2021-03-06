﻿using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Stateless;
using UnityEngine;
using static GameJam.Utils;
using Random = UnityEngine.Random;

namespace GameJam
{
	public class UnitStateMachine
	{
		public enum States { Inactive, PlayerControl, Helper, MoveInPosition, IdleShooter, Roamer, Destroy }
		public enum Triggers { Done, Destroyed }

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
				{ States.PlayerControl, new PlayerControlState(this, game, actor) },
				{ States.Helper, new HelperState(this, game, actor) },
				{ States.MoveInPosition, new MoveInPositionState(this, game, actor) },
				{ States.IdleShooter, new ShooterState(this, game, actor) },
				{ States.Roamer, new RoamerState(this, game, actor) },
				{ States.Destroy, new DestroyState(this, game, actor) },
			};

			_machine = new StateMachine<States, Triggers>(States.Inactive);

			_machine.Configure(States.Inactive)
				.PermitDynamicIf(Triggers.Done, () => States.MoveInPosition, () => _actor.Brain == Brain.Player)
				.PermitDynamicIf(Triggers.Done, () => States.Helper, () => _actor.Brain == Brain.Helper)
				.PermitDynamicIf(Triggers.Done, () => States.MoveInPosition, () => _actor.Brain == Brain.Shooter)
				.PermitDynamicIf(Triggers.Done, () => States.MoveInPosition, () => _actor.Brain == Brain.Roamer);

			_machine.Configure(States.MoveInPosition)
				.PermitDynamicIf(Triggers.Done, () => States.PlayerControl, () => _actor.Brain == Brain.Player)
				.PermitDynamicIf(Triggers.Done, () => States.IdleShooter, () => _actor.Brain == Brain.Shooter)
				.PermitDynamicIf(Triggers.Done, () => States.Roamer, () => _actor.Brain == Brain.Roamer)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.Configure(States.Helper)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.Configure(States.Roamer)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.Configure(States.IdleShooter)
				.Permit(Triggers.Destroyed, States.Destroy);

			_machine.Configure(States.PlayerControl)
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
				Debug.LogWarning("Invalid transition " + _currentState.GetType().Name + " -> " + trigger);
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

			public virtual void Tick() { }
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

		private class PlayerControlState : BaseEntityState
		{
			private Vector3 _defaultMousePosition;

			public PlayerControlState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_defaultMousePosition = GetMouseWorldPosition(_game.Controls, _game.Camera);
			}

			public override void Tick()
			{
				base.Tick();

				var mouseWorldPosition = GetMouseWorldPosition(_game.Controls, _game.Camera);
				var moveInput = _game.Controls.Gameplay.Move.ReadValue<Vector2>();
				var confirmInput = _game.Controls.Gameplay.Confirm.ReadValue<float>();

				if (moveInput.magnitude > 0f)
				{
					var destination = _actor.transform.position + new Vector3(moveInput.x, moveInput.y, 0f);
					destination.x = Mathf.Clamp(destination.x, Game.MoveBounds.min.x, Game.MoveBounds.max.x);
					destination.y = Mathf.Clamp(destination.y, Game.MoveBounds.min.y, Game.MoveBounds.max.y);
					_actor.transform.position = Vector3.Lerp(_actor.transform.position, destination, _actor.MoveSpeed * Time.deltaTime);
				}

				if (mouseWorldPosition != _defaultMousePosition)
				{
					_actor.transform.up = Vector3.Lerp(_actor.transform.up, mouseWorldPosition - _actor.transform.position, 25f * Time.deltaTime);
				}

				_actor.HelperAngle += _actor.RotateSpeed * Time.deltaTime;

				if (confirmInput > 0f)
				{
					FireProjectile(_actor, _game.State, _game.AudioPlayer, _game.ProjectileSpawner);

					foreach (var helper in _game.State.Helpers)
					{
						FireProjectile(helper, _game.State, _game.AudioPlayer, _game.ProjectileSpawner);
					}
				}
			}
		}

		private class MoveInPositionState : BaseEntityState
		{
			public MoveInPositionState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				if (_actor.transform.position != _actor.MoveDestination)
				{
					await _actor.transform.DOMove(_actor.MoveDestination, 3f).SetEase(Ease.InSine);
				}

				_machine.Fire(Triggers.Done);
			}
		}

		private class HelperState : BaseEntityState
		{
			private bool _circlingPlayer;
			private double _startCirclingPlayerTimestamp;

			public HelperState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_startCirclingPlayerTimestamp = Time.time + 2f;
			}

			public override void Tick()
			{
				base.Tick();

				var radius = Game.HELPERS_RADIUS;
				var angle = _game.State.Player.HelperAngle + _actor.HelperIndex * Mathf.PI * 2 / Game.HELPERS_MAX_COUNT;
				var offset = new Vector3(
					Mathf.Cos(angle) * radius,
					Mathf.Sin(angle) * radius,
					0f
				);
				var destination = _game.State.Player.transform.position + offset;

				 if (_circlingPlayer)
				 {
					 _actor.transform.position = destination;
				 }
				 else
				 {
					 _actor.transform.position = Vector3.Lerp(_actor.transform.position, destination, _actor.MoveSpeed * Time.deltaTime);

					 if (Vector3.Distance(_actor.transform.position, destination) < 0.2f || Time.time > _startCirclingPlayerTimestamp)
					 {
						 _circlingPlayer = true;
					 }
				 }

				 _actor.transform.rotation = _game.State.Player.transform.rotation;
			}
		}

		private class ShooterState : BaseEntityState
		{
			public ShooterState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override void Tick()
			{
				base.Tick();

				foreach (var shooter in _actor.Shooters)
				{
					shooter.transform.Rotate(_actor.RotationPerTick * Time.deltaTime);
				}

				FireProjectile(_actor, _game.State, _game.AudioPlayer, _game.ProjectileSpawner);
			}
		}

		private class RoamerState : BaseEntityState
		{
			private Vector3 _destination;

			public RoamerState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				_destination = _actor.transform.position;
			}

			public override void Tick()
			{
				base.Tick();

				if (Vector3.Distance(_destination, _actor.transform.position) < .1f)
				{
					var destination = _actor.transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0f);
					if (Game.MoveBounds.Contains(destination))
					{
						_destination = destination;
					}
				}

				if (_game.State.Player != null)
				{
					foreach (var shooter in _actor.Shooters)
					{
						var randomOffset = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
						shooter.transform.up = (_game.State.Player.transform.position + randomOffset) - _actor.transform.position;
					}
				}

				_actor.transform.position = Vector3.Lerp(_actor.transform.position, _destination, Time.deltaTime * _actor.MoveSpeed);

				FireProjectile(_actor, _game.State, _game.AudioPlayer, _game.ProjectileSpawner);
			}
		}

		private class DestroyState : BaseEntityState
		{
			public DestroyState(UnitStateMachine machine, Game game, EntityComponent actor) : base(machine, game, actor) { }

			public override async UniTask Enter()
			{
				await base.Enter();

				if (_actor.DestroyedClip)
				{
					_ = _game.AudioPlayer.PlaySoundEffect(_actor.DestroyedClip);
				}

				if (_actor.DestroyedParticle)
				{
					GameObject.Instantiate(_actor.DestroyedParticle, _actor.transform.position, Quaternion.identity);
					await UniTask.Delay(TimeSpan.FromSeconds(_actor.DestroyedParticle.main.duration));
				}

				if (_actor.Brain == Brain.Helper)
				{
					_ = _game.UI.ShowNotification(_actor.Name);
				}

				if (_actor == _game.State.Player)
				{
					foreach (var helper in _game.State.Helpers)
					{
						if (helper != null)
						{
							GameObject.Destroy(helper.gameObject);
						}
					}
				}

				GameObject.Destroy(_actor.gameObject);
			}
		}
	}
}

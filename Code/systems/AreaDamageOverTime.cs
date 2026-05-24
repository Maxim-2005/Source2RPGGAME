using System;
using System.Collections.Generic;
using Sandbox;

namespace MagicSystem
{
	public abstract class AreaDamageOverTime : Component
	{
		[Property] public float DamagePerTick { get; set; }
		[Property] public float TickInterval { get; set; }
		[Property] public float Radius { get; set; }
		[Property] public float Lifetime { get; set; }
		[Property] public bool ShowDebug { get; set; } = true;
		[Hide] public GameObject Launcher { get; set; }

		protected abstract string DebugPrefix { get; }
		protected abstract Color DebugColor { get; }

		private RealTimeSince _timeSinceSpawn;
		private Dictionary<GameObject, float> _targetTimers = new();

		protected override void OnStart()
		{
			_timeSinceSpawn = 0f;
			ValidateDefaults();
			if ( ShowDebug )
				Log.Info( $"[{DebugPrefix}] Скрипт {GetType().Name} запущен! Радиус: {Radius}" );
		}

		protected virtual void ValidateDefaults()
		{
			if ( Radius <= 0f ) Radius = 120f;
			if ( TickInterval <= 0f ) TickInterval = 0.5f;
		}

		protected override void OnUpdate()
		{
			if ( _timeSinceSpawn >= Lifetime )
			{
				GameObject.Destroy();
				return;
			}

			if ( ShowDebug )
			{
				using ( Gizmo.Scope() )
				{
					Gizmo.Draw.Color = DebugColor;
					DrawDebugGizmos( GameObject.WorldPosition, Radius );
				}
			}

			GameObject launcher = Launcher;
			Vector3 zonePos = GameObject.WorldPosition;

			Vector3 traceEnd = zonePos + GetTraceEndOffset();
			var hits = Scene.PhysicsWorld.Trace
				.Sphere( Radius, zonePos, traceEnd )
				.WithTag( GameTags.Enemy )
				.RunAll();

			HashSet<GameObject> currentValidTargets = new();
			float deltaTime = Time.Delta;

			var activeKeys = new List<GameObject>( _targetTimers.Keys );
			foreach ( var key in activeKeys )
			{
				if ( key.IsValid() )
					_targetTimers[key] += deltaTime;
			}

			if ( hits != null )
			{
				foreach ( var hit in hits )
				{
					GameObject target = hit.Body?.GameObject;
					if ( target == null || !target.IsValid() ) continue;
					if ( target.IsOwnedBy( launcher ) ) continue;
					if ( !IsTargetInZone( zonePos, target, Radius ) ) continue;

					currentValidTargets.Add( target );

					if ( !_targetTimers.ContainsKey( target ) )
					{
						_targetTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
					else if ( _targetTimers[target] >= TickInterval )
					{
						_targetTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
				}
			}

			foreach ( var key in activeKeys )
			{
				if ( !currentValidTargets.Contains( key ) || !key.IsValid() )
					_targetTimers.Remove( key );
			}
		}

		protected abstract void DrawDebugGizmos( Vector3 position, float radius );
		protected abstract bool IsTargetInZone( Vector3 zonePos, GameObject target, float radius );
		protected virtual Vector3 GetTraceEndOffset() => Vector3.Zero;

		private void ApplyDamage( GameObject target, GameObject launcher )
		{
			if ( ShowDebug )
				Log.Info( $"[{DebugPrefix}] Урон по: {target.Name} от {launcher?.Name}" );

			DamageService.ApplyDamage( target, DamagePerTick, launcher );
		}
	}
}

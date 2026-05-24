using Sandbox;
using MagicSystem;
using System.Collections.Generic;
using System;

public sealed class MagicProjectile : Component
{
	private Vector3 _direction;
	private GameObject _launcher;
	private AttackProjectile _config;
	private bool _launched = false;

	private bool _isTracerMode = false;
	private float _currentSpeed = 2500f;
	private Vector3 _startPosition;
	private float? _maxFlightDistance;

	private HashSet<Guid> _hitTargetsThisSpawn = new HashSet<Guid>();

	public void LaunchAsDirect( GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance = null )
	{
		_launcher = launcher;
		_config = config;
		_isTracerMode = false;
		_direction = direction.Normal;
		_startPosition = GameObject.WorldPosition;

		_maxFlightDistance = flightDistance;
		_currentSpeed = config.MagicType == ProjectileType.Direct ? config.DirectMode.Speed : config.MeteorMode.FallSpeed;

		float scale = config.MagicType == ProjectileType.Direct ? config.DirectMode.ProjectileScale : config.MeteorMode.Scale;
		var visuals = GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
		visuals?.SetupVisuals( config.MagicType, scale );

		_launched = true;
	}

	public void LaunchAsMeteorTracer( GameObject launcher, Vector3 direction, AttackProjectile config, float flightDistance )
	{
		_launcher = launcher;
		_direction = direction.Normal;
		_config = config;
		_isTracerMode = true;
		_currentSpeed = 150000f;
		_startPosition = GameObject.WorldPosition;
		_maxFlightDistance = flightDistance;
		_launched = true;

		var visuals = GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
		visuals?.HideAll();
	}

	protected override void OnUpdate()
	{
		if ( !_launched || _config == null ) return;

		float travelDistance = Vector3.DistanceBetween( _startPosition, GameObject.WorldPosition );

		if ( _maxFlightDistance.HasValue && travelDistance >= _maxFlightDistance.Value )
		{
			HandleMaxDistanceReached();
			return;
		}

		Vector3 currentPos = GameObject.WorldPosition;
		float step = _currentSpeed * Time.Delta;

		if ( _maxFlightDistance.HasValue )
		{
			float remaining = _maxFlightDistance.Value - travelDistance;
			if ( step > remaining )
				step = Math.Max( remaining, 0f );
		}

		if ( step <= 0f )
		{
			HandleMaxDistanceReached();
			return;
		}

		Vector3 nextPosition = currentPos + _direction * step;

		if ( !_isTracerMode && _config.MagicType == ProjectileType.Meteor )
		{
			float radius = 16f * _config.MeteorMode.Scale;
			var tr = Scene.PhysicsWorld.Trace
				.Sphere( radius, GameObject.WorldPosition, nextPosition )
				.WithoutTags( GameTags.Projectile, "trigger" )
				.Run();

			if ( tr.Hit && tr.Body?.GameObject != null )
			{
				var hitGo = tr.Body.GameObject;

				if ( hitGo.IsOwnedBy( _launcher ) )
				{
					tr.Hit = false;
				}
				else if ( hitGo.Tags.Has( "player" ) || hitGo.Tags.Has( GameTags.Enemy ) )
				{
					if ( _config.MeteorMode.HasDirectHit && hitGo.Tags.Has( GameTags.Enemy ) )
					{
						if ( !_hitTargetsThisSpawn.Contains( hitGo.Id ) )
						{
							_hitTargetsThisSpawn.Add( hitGo.Id );
							DamageService.ApplyDamage( hitGo, _config.MeteorMode.Damage, _launcher );
						}
					}
					tr.Hit = false;
				}
			}

			if ( tr.Hit ) { Impact( tr ); return; }
		}
		else
		{
			var tr = Scene.PhysicsWorld.Trace
				.Ray( GameObject.WorldPosition, nextPosition )
				.WithoutTags( GameTags.Projectile, "trigger" )
				.Run();

			if ( tr.Hit ) { Impact( tr ); return; }
		}

		GameObject.WorldPosition = nextPosition;
	}

	private void HandleMaxDistanceReached()
	{
		if ( _isTracerMode )
		{
			Vector3 targetFloorPos = GameObject.WorldPosition;
			Vector3 skySpawnPos = targetFloorPos + ( Vector3.Up * _config.MeteorMode.SpawnHeight );
			skySpawnPos -= _direction * ( _config.MeteorMode.SpawnHeight * 0.4f );
			Vector3 fallDirection = (targetFloorPos - skySpawnPos).Normal;
			var meteorGo = _config.ProjectilePrefab.Clone( skySpawnPos, Rotation.LookAt( fallDirection ) );
			var meteorScript = meteorGo.Components.Get<MagicProjectile>();
			meteorScript?.LaunchAsDirect( _launcher, fallDirection, _config );
			GameObject.Destroy();
		}
		else
		{
			TriggerAirExplosion();
		}
	}

	private void TriggerAirExplosion()
	{
		PhysicsTraceResult airExplosionTrace = new PhysicsTraceResult
		{
			Hit = true,
			EndPosition = GameObject.WorldPosition,
			Normal = -_direction
		};
		Impact( airExplosionTrace );
	}

	private void Impact( PhysicsTraceResult tr )
	{
		GameObject hitTarget = tr.Body?.GameObject;
		if ( hitTarget.IsOwnedBy( _launcher ) ) return;

		HandleDirectHitDamage( hitTarget );

		if ( _isTracerMode )
		{
			SpawnMeteorFromTracer( tr.EndPosition );
			GameObject.Destroy();
			return;
		}

		if ( ShouldSpawnRollingBoulder() )
			SpawnRollingBoulder( tr.EndPosition );

		SpawnZoneEffects( tr.EndPosition );
		GameObject.Destroy();
	}

	private void HandleDirectHitDamage( GameObject hitTarget )
	{
		if ( _config.MagicType != ProjectileType.Direct || !_config.DirectMode.HasDirectHit ) return;
		if ( hitTarget == null || !hitTarget.Tags.Has( GameTags.Enemy ) ) return;

		DamageService.ApplyDamage( hitTarget, _config.DirectMode.Damage, _launcher );
	}

	private void SpawnMeteorFromTracer( Vector3 impactPosition )
	{
		using ( Gizmo.Scope() ) { }

		Vector3 skySpawnPos = impactPosition + ( Vector3.Up * _config.MeteorMode.SpawnHeight );
		skySpawnPos -= _direction * ( _config.MeteorMode.SpawnHeight * 0.4f );

		Vector3 fallDirection = ( impactPosition - skySpawnPos ).Normal;
		var meteorGo = _config.ProjectilePrefab.Clone( skySpawnPos, Rotation.LookAt( fallDirection ) );
		var meteorScript = meteorGo.Components.Get<MagicProjectile>();
		meteorScript?.LaunchAsDirect( _launcher, fallDirection, _config );
	}

	private bool ShouldSpawnRollingBoulder()
	{
		return _config.MagicType == ProjectileType.Meteor
			&& _config.MeteorMode.RollAfterImpact
			&& _config.MeteorMode.RollingPrefab != null;
	}

	private void SpawnRollingBoulder( Vector3 impactPosition )
	{
		var currentVisuals = GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
		currentVisuals?.HideAll();

		var rootRenderer = GameObject.Components.Get<ModelRenderer>();
		if ( rootRenderer != null ) rootRenderer.Enabled = false;

		Vector3 rollDir = new Vector3( _direction.x, _direction.y, 0 ).Normal;

		var rollingGo = _config.MeteorMode.RollingPrefab.Clone( impactPosition, Rotation.Identity );
		rollingGo.WorldScale = _config.MeteorMode.Scale;

		var rollingScript = rollingGo.Components.Get<MeteorRollingLogic>();
		rollingScript?.InitializeRoll( _launcher, rollDir, _config, 16f );

		var spawnerScript = rollingGo.Components.Get<ObjectTrailSpawner>();
		if ( spawnerScript != null )
		{
			spawnerScript.SpawnInterval = _config.MeteorMode.TrailSpawnInterval;
		}
	}

	private void SpawnZoneEffects( Vector3 position )
	{
		if ( _config.ZonePrefab == null ) return;
		if ( !_config.Explosion.Enabled && !_config.Puddle.Enabled && !_config.Gas.Enabled ) return;

		var zoneGo = _config.ZonePrefab.Clone( position, Rotation.Identity );
		ZoneFactory.ConfigureZoneEffects( zoneGo, _config, _launcher );
	}
}

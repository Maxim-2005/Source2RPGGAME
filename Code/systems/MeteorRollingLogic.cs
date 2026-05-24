using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using MagicSystem;

public sealed class MeteorRollingLogic : Component, IGroundTraceable, ITrailEffectProvider
{
	private const float InAirThreshold = 0.1f;
	private const float GroundRaycastDistance = 1000f;
	private const float VerticalTolerance = 5f;
	private const float MaxSlopeNormalZ = 0.7071f;
	private const float GravityForce = 981f;
	private const float KillPlaneZ = -3000f;
	private const float DamageRadiusMultiplier = 1.2f;
	private const float PushVerticalFactor = 0.5f;

	[Hide] private Vector3 _rollDirection;
	[Hide] private float _speed;
	[Hide] private float _lifetime;
	[Hide] private float _radius;
	[Hide] private float _rollDamage;
	[Hide] private RealTimeSince _timeSinceSpawn;
	[Hide] private bool _isInitialized = false;

	[Hide] private GameObject _visualsFolder;
	[Hide] private float _fallVelocity = 0f;
	[Hide] private PuddleSettings _puddleConfig;
	[Hide] private GasSettings _gasConfig;
	[Hide] private GameObject _launcher;

	[Hide] private HashSet<Guid> _hitTargets = new HashSet<Guid>();

	[Property] public float VerticalOffset { get; set; } = 32f;
	[Property] public float SmoothSpeed { get; set; } = 25f;
	[Property] public float PushForce { get; set; } = 450f;

	public bool IsInAir => _fallVelocity > InAirThreshold;

	public TrailSettings GetTrailSettings() => new() { Puddle = _puddleConfig, Gas = _gasConfig };
	public GameObject GetLauncher() => _launcher;

	public void InitializeRoll( GameObject launcher, Vector3 direction, AttackProjectile config, float radius )
	{
		_launcher = launcher;
		_rollDirection = direction.WithZ( 0 ).Normal;
		_speed = config.MeteorMode.RollSpeed;
		_lifetime = config.MeteorMode.RollDuration;
		_radius = radius;
		_rollDamage = config.MeteorMode.RollDamage;
		_puddleConfig = config.Puddle;
		_gasConfig = config.Gas;

		_visualsFolder = GameObject.Children.FirstOrDefault( c => c.Name == "Visuals" ) ?? GameObject;

		_timeSinceSpawn = 0f;
		_isInitialized = true;
	}

	protected override void OnUpdate()
	{
		if ( !_isInitialized ) return;

		if ( _timeSinceSpawn >= _lifetime )
		{
			GameObject.Destroy();
			return;
		}

		UpdateRotation();
		UpdateMovement();
		ApplyOverlapDamage();
	}

	private void UpdateRotation()
	{
		if ( _visualsFolder == null || _visualsFolder == GameObject ) return;

		float rotationAngle = (_speed * Time.Delta) / _radius * (180f / (float)Math.PI);
		Vector3 rotationAxis = Vector3.Cross( Vector3.Up, _rollDirection ).Normal;
		_visualsFolder.LocalRotation *= Rotation.FromAxis( rotationAxis, rotationAngle );
	}

	private void UpdateMovement()
	{
		Vector3 currentPos = GameObject.WorldPosition;
		float scaledRadius = _radius * GameObject.WorldScale.x;
		Vector3 nextPosHorizontal = currentPos + _rollDirection * _speed * Time.Delta;

		Vector3 rayStart = nextPosHorizontal + Vector3.Up * VerticalOffset;
		Vector3 rayEnd = new Vector3( nextPosHorizontal.x, nextPosHorizontal.y, currentPos.z - GroundRaycastDistance );

		var terrainTrace = Scene.PhysicsWorld.Trace
			.Ray( rayStart, rayEnd )
			.WithAnyTags( "world", "solid", "map", "static" )
			.WithoutTags( GameTags.Player, GameTags.Projectile, GameTags.Trigger, GameTags.Enemy )
			.Run();

		if ( terrainTrace.Hit )
		{
			HandleTerrainHit( terrainTrace, currentPos, nextPosHorizontal, scaledRadius );
		}
		else
		{
			HandleFall( currentPos, nextPosHorizontal );
		}
	}

	private void HandleTerrainHit( PhysicsTraceResult terrainTrace, Vector3 currentPos, Vector3 nextPosHorizontal, float scaledRadius )
	{
		float targetZ = terrainTrace.EndPosition.z + scaledRadius;

		if ( terrainTrace.EndPosition.z > (currentPos.z - scaledRadius + VerticalTolerance) )
		{
			if ( terrainTrace.Normal.z < MaxSlopeNormalZ )
			{
				var terrainGo = terrainTrace.Body?.GameObject;
				var checkRb = terrainGo?.Components.Get<Rigidbody>();

				if ( checkRb == null )
				{
					GameObject.Destroy();
					return;
				}
			}
		}

		_fallVelocity = 0f;
		float smoothedZ = MathX.Lerp( currentPos.z, targetZ, SmoothSpeed * Time.Delta );
		GameObject.WorldPosition = new Vector3( nextPosHorizontal.x, nextPosHorizontal.y, smoothedZ );
	}

	private void HandleFall( Vector3 currentPos, Vector3 nextPosHorizontal )
	{
		_fallVelocity += GravityForce * Time.Delta;
		float fallZ = currentPos.z - _fallVelocity * Time.Delta;
		GameObject.WorldPosition = new Vector3( nextPosHorizontal.x, nextPosHorizontal.y, fallZ );

		if ( GameObject.WorldPosition.z < KillPlaneZ )
		{
			GameObject.Destroy();
		}
	}

	private void ApplyOverlapDamage()
	{
		float scaledRadius = _radius * GameObject.WorldScale.x;
		float damageRadius = scaledRadius * DamageRadiusMultiplier;
		var overlapTraces = Scene.PhysicsWorld.Trace
			.Sphere( damageRadius, GameObject.WorldPosition, GameObject.WorldPosition )
			.WithTag( GameTags.Enemy )
			.WithoutTags( GameTags.Projectile, GameTags.Trigger, GameTags.Player )
			.RunAll();

		if ( overlapTraces == null ) return;

		foreach ( var tr in overlapTraces )
		{
			if ( tr.Body?.GameObject == null ) continue;
			var enemyGo = tr.Body.GameObject;

			if ( _hitTargets.Contains( enemyGo.Id ) ) continue;

			DamageService.ApplyDamage( enemyGo, _rollDamage, _launcher );
			_hitTargets.Add( enemyGo.Id );

			var rb = enemyGo.Components.Get<Rigidbody>();
			if ( rb != null )
			{
				Vector3 pushDir = (enemyGo.WorldPosition - GameObject.WorldPosition).WithZ( PushVerticalFactor ).Normal;
				rb.Velocity += pushDir * PushForce;
			}
		}
	}
}

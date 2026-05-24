using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using MagicSystem;

public sealed class MeteorRollingLogic : Component, IGroundTraceable, ITrailEffectProvider
{
	private Vector3 _rollDirection;
	private float _speed;
	private float _lifetime;
	private float _radius;
	private float _rollDamage;
	private RealTimeSince _timeSinceSpawn;
	private bool _isInitialized = false;

	private GameObject _visualsFolder;
	private float _fallVelocity = 0f;
	private PuddleSettings _puddleConfig;
	private GasSettings _gasConfig;
	private GameObject _launcher;

	private HashSet<Guid> _hitTargets = new HashSet<Guid>();

	[Property] public float VerticalOffset { get; set; } = 32f;
	[Property] public float SmoothSpeed { get; set; } = 25f;
	[Property] public float PushForce { get; set; } = 450f;

	public bool IsInAir => _fallVelocity > 0.1f;

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

		if ( _visualsFolder != null && _visualsFolder != GameObject )
		{
			float rotationAngle = (_speed * Time.Delta) / _radius * (180f / (float)Math.PI);
			Vector3 rotationAxis = Vector3.Cross( Vector3.Up, _rollDirection ).Normal;
			_visualsFolder.LocalRotation *= Rotation.FromAxis( rotationAxis, rotationAngle );
		}

		Vector3 currentPos = GameObject.WorldPosition;
		float scaledRadius = _radius * GameObject.WorldScale.x;
		Vector3 nextPosHorizontal = currentPos + _rollDirection * _speed * Time.Delta;

		Vector3 rayStart = nextPosHorizontal + Vector3.Up * VerticalOffset;
		Vector3 rayEnd = new Vector3( nextPosHorizontal.x, nextPosHorizontal.y, currentPos.z - 1000f );

		var terrainTrace = Scene.PhysicsWorld.Trace
			.Ray( rayStart, rayEnd )
			.WithAnyTags( "world", "solid", "map", "static" )
			.WithoutTags( "player", GameTags.Projectile, "trigger", GameTags.Enemy )
			.Run();

		if ( terrainTrace.Hit )
		{
			float targetZ = terrainTrace.EndPosition.z + scaledRadius;

			if ( terrainTrace.EndPosition.z > (currentPos.z - scaledRadius + 5f) )
			{
				if ( terrainTrace.Normal.z < 0.7071f )
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
		else
		{
			_fallVelocity += 981f * Time.Delta;
			float fallZ = currentPos.z - _fallVelocity * Time.Delta;
			GameObject.WorldPosition = new Vector3( nextPosHorizontal.x, nextPosHorizontal.y, fallZ );

			if ( GameObject.WorldPosition.z < -3000f )
			{
				GameObject.Destroy();
				return;
			}
		}

		float damageRadius = scaledRadius * 1.2f;
		var overlapTraces = Scene.PhysicsWorld.Trace
			.Sphere( damageRadius, GameObject.WorldPosition, GameObject.WorldPosition )
			.WithTag( GameTags.Enemy )
			.WithoutTags( GameTags.Projectile, "trigger", "player" )
			.RunAll();

		if ( overlapTraces != null )
		{
			foreach ( var tr in overlapTraces )
			{
				if ( tr.Body?.GameObject == null ) continue;
				var enemyGo = tr.Body.GameObject;

				if ( !_hitTargets.Contains( enemyGo.Id ) )
				{
					var health = enemyGo.GetHealth();
					if ( health != null )
					{
						health.TakeDamage( _rollDamage, _launcher );
						_hitTargets.Add( enemyGo.Id );

						var rb = enemyGo.Components.Get<Rigidbody>();
						if ( rb != null )
						{
							Vector3 pushDir = (enemyGo.WorldPosition - GameObject.WorldPosition).WithZ( 0.5f ).Normal;
							rb.Velocity += pushDir * PushForce;
						}
					}
				}
			}
		}
	}
}

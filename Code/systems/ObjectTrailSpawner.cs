using System;
using Sandbox;
using MagicSystem;

public sealed class ObjectTrailSpawner : Component
{
	[Property] public PrefabScene TrailPrefab { get; set; }

	// Ётот интервал будет перезаписан из MagicProjectile при спавне
	[Property, Group( "Settings" )] public float SpawnInterval { get; set; } = 0.18f;

	private RealTimeSince _timeSinceLastSpawn;
	private IGroundTraceable _traceableObject;
	private MeteorRollingLogic _meteorLogic;

	protected override void OnStart()
	{
		_timeSinceLastSpawn = 0;
		_traceableObject = GameObject.Components.Get<IGroundTraceable>();
		_meteorLogic = GameObject.Components.Get<MeteorRollingLogic>();
	}

	protected override void OnUpdate()
	{
		if ( TrailPrefab == null ) return;
		if ( _timeSinceLastSpawn < SpawnInterval ) return;
		if ( _traceableObject != null && _traceableObject.IsInAir ) return;

		// «ащита: провер€ем, включены ли лужа или газ в структурах
		if ( _meteorLogic != null )
		{
			PuddleSettings puddleConfig = _meteorLogic.GetPuddleConfig();
			GasSettings gasConfig = _meteorLogic.GetGasConfig();

			if ( !puddleConfig.Enabled && !gasConfig.Enabled ) return;
		}

		SpawnTrailObject();
		_timeSinceLastSpawn = 0;
	}

	private void SpawnTrailObject()
	{
		Vector3 spawnPosition = GameObject.WorldPosition;

		var downTrace = Scene.PhysicsWorld.Trace
			.Ray( spawnPosition, spawnPosition + Vector3.Down * 200f )
			.WithAnyTags( "world", "solid", "map", "static" )
			.WithoutTags( "player", "projectile", "trigger" )
			.Run();

		Vector3 finalSpawnPos = downTrace.Hit ? downTrace.EndPosition : spawnPosition;

		var zoneGo = TrailPrefab.Clone( finalSpawnPos, Rotation.Identity );
		if ( zoneGo == null ) return;

		GameObject activeLauncher = _meteorLogic?.GetLauncher();
		var baseZone = zoneGo.Components.GetOrCreate<ExplosionBase>();

		if ( baseZone != null && activeLauncher != null )
		{
			baseZone.SetupZone( activeLauncher );
		}

		if ( _meteorLogic != null )
		{
			PuddleSettings puddleConfig = _meteorLogic.GetPuddleConfig();
			if ( puddleConfig.Enabled )
			{
				var puddle = zoneGo.Components.GetOrCreate<FirePuddleDamage>();
				if ( puddle != null )
				{
					puddle.DamagePerTick = puddleConfig.DamagePerTick;
					puddle.TickInterval = puddleConfig.TickInterval;
					puddle.Radius = puddleConfig.Radius;
					puddle.MaxHeight = puddleConfig.PuddleHeight;
					puddle.PuddleLifetime = puddleConfig.Lifetime;
				}
			}

			GasSettings gasConfig = _meteorLogic.GetGasConfig();
			if ( gasConfig.Enabled )
			{
				var gas = zoneGo.Components.GetOrCreate<GasCloudDamage>();
				if ( gas != null )
				{
					gas.DamagePerTick = gasConfig.DamagePerTick;
					gas.TickInterval = gasConfig.TickInterval;
					gas.Radius = gasConfig.Radius;
					gas.CloudLifetime = gasConfig.Lifetime;
				}
			}
		}
	}
}

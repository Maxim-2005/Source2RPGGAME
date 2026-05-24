using System;
using Sandbox;
using MagicSystem;

public sealed class ObjectTrailSpawner : Component
{
	[Property] public PrefabScene TrailPrefab { get; set; }

	[Property, Group( "Settings" )] public float SpawnInterval { get; set; } = 0.18f;

	[Hide] private RealTimeSince _timeSinceLastSpawn;
	[Hide] private IGroundTraceable _traceableObject;
	[Hide] private ITrailEffectProvider _trailProvider;

	protected override void OnStart()
	{
		_timeSinceLastSpawn = 0;
		_traceableObject = GameObject.Components.Get<IGroundTraceable>();
		_trailProvider = GameObject.Components.Get<ITrailEffectProvider>();
	}

	protected override void OnUpdate()
	{
		if ( TrailPrefab == null ) return;
		if ( _timeSinceLastSpawn < SpawnInterval ) return;
		if ( _traceableObject != null && _traceableObject.IsInAir ) return;

		if ( _trailProvider != null )
		{
			TrailSettings trailSettings = _trailProvider.GetTrailSettings();
			if ( !trailSettings.HasAnyEnabled ) return;
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
			.WithoutTags( GameTags.Player, GameTags.Projectile, GameTags.Trigger )
			.Run();

		Vector3 finalSpawnPos = downTrace.Hit ? downTrace.EndPosition : spawnPosition;

		var zoneGo = TrailPrefab.Clone( finalSpawnPos, Rotation.Identity );
		if ( zoneGo == null ) return;

		if ( _trailProvider == null ) return;

		TrailSettings trailSettings = _trailProvider.GetTrailSettings();
		GameObject activeLauncher = _trailProvider.GetLauncher();
		ZoneFactory.ConfigureZoneEffects( zoneGo, trailSettings, activeLauncher );
	}
}

using System;
using Sandbox;

namespace MagicSystem
{
	public enum ProjectileType
	{
		Direct,
		Meteor
	}

	[System.Serializable]
	public record struct DirectSettings
	{
		public DirectSettings() { }
		[Property] public float Damage { get; set; } = 25f;
		[Property] public float Speed { get; set; } = 1200f;
		[Property] public float ProjectileScale { get; set; } = 1f;
		[Property] public bool HasDirectHit { get; set; } = true;
	}

	[System.Serializable]
	public record struct MeteorSettings
	{
		public MeteorSettings() { }
		[Property] public float Damage { get; set; } = 60f;
		[Property] public float SpawnHeight { get; set; } = 900f;
		[Property] public float FallSpeed { get; set; } = 3000f;
		[Property] public float Scale { get; set; } = 2.5f;
		[Property] public bool HasDirectHit { get; set; } = true;
		[Property] public bool RollAfterImpact { get; set; } = true;
		[Property] public GameObject RollingPrefab { get; set; }
		[Property] public float RollSpeed { get; set; } = 450f;
		[Property] public float RollDuration { get; set; } = 3.5f;
		[Property] public float RollDamage { get; set; } = 15f;
		[Property, Title( "Trail Spawn Interval" )] public float TrailSpawnInterval { get; set; } = 0.18f;
	}

	[System.Serializable]
	public record struct ExplosionSettings
	{
		public ExplosionSettings() { }
		[Property] public bool Enabled { get; set; } = true;
		[Property] public float Damage { get; set; } = 40f;
		[Property] public float Radius { get; set; } = 180f;
		[Property] public float DebugTime { get; set; } = 2f;
	}

	[System.Serializable]
	public record struct PuddleSettings
	{
		public PuddleSettings() { }
		[Property] public bool Enabled { get; set; } = false;
		[Property] public float DamagePerTick { get; set; } = 8f;
		[Property] public float TickInterval { get; set; } = 0.5f;
		[Property] public float Radius { get; set; } = 100f;
		[Property] public float PuddleHeight { get; set; } = 30f;
		[Property] public float Lifetime { get; set; } = 5f;
	}

	[System.Serializable]
	public record struct GasSettings
	{
		public GasSettings() { }
		[Property] public bool Enabled { get; set; } = false;
		[Property] public float DamagePerTick { get; set; } = 12f;
		[Property] public float TickInterval { get; set; } = 0.4f;
		[Property] public float Radius { get; set; } = 140f;
		[Property] public float Lifetime { get; set; } = 6f;
	}

	[System.Serializable]
	public record struct StatusEffectApply
	{
		public StatusEffectApply() { }
		public string Id { get; set; } = "burn";
		public float Duration { get; set; } = 3f;
		public float Magnitude { get; set; } = 5f;
		public float TickInterval { get; set; } = 1f;
	}

	public record struct TrailSettings
	{
		public PuddleSettings Puddle { get; set; }
		public GasSettings Gas { get; set; }
		public bool HasAnyEnabled => Puddle.Enabled || Gas.Enabled;
	}
}

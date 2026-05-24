using Sandbox;
using MagicSystem;

public static class ZoneFactory
{
	public static void ConfigureZoneEffects( GameObject zoneGo, AttackProjectile config, GameObject launcher )
	{
		if ( config.Explosion.Enabled )
		{
			var exp = zoneGo.Components.GetOrCreate<AoEExplosionDamage>();
			exp.Damage = config.Explosion.Damage;
			exp.Radius = config.Explosion.Radius;
			exp.ExplosionDebugLifetime = config.Explosion.DebugTime;
			exp.Launcher = launcher;
			exp.Explode();
		}

		if ( config.Puddle.Enabled )
		{
			var puddle = zoneGo.Components.GetOrCreate<FirePuddleDamage>();
			puddle.DamagePerTick = config.Puddle.DamagePerTick;
			puddle.TickInterval = config.Puddle.TickInterval;
			puddle.Radius = config.Puddle.Radius;
			puddle.MaxHeight = config.Puddle.PuddleHeight;
			puddle.Lifetime = config.Puddle.Lifetime;
			puddle.Launcher = launcher;
		}

		if ( config.Gas.Enabled )
		{
			var gas = zoneGo.Components.GetOrCreate<GasCloudDamage>();
			gas.DamagePerTick = config.Gas.DamagePerTick;
			gas.TickInterval = config.Gas.TickInterval;
			gas.Radius = config.Gas.Radius;
			gas.Lifetime = config.Gas.Lifetime;
			gas.Launcher = launcher;
		}
	}

	public static void ConfigureZoneEffects( GameObject zoneGo, TrailSettings trailSettings, GameObject launcher )
	{
		if ( trailSettings.Puddle.Enabled )
		{
			var puddle = zoneGo.Components.GetOrCreate<FirePuddleDamage>();
			puddle.DamagePerTick = trailSettings.Puddle.DamagePerTick;
			puddle.TickInterval = trailSettings.Puddle.TickInterval;
			puddle.Radius = trailSettings.Puddle.Radius;
			puddle.MaxHeight = trailSettings.Puddle.PuddleHeight;
			puddle.Lifetime = trailSettings.Puddle.Lifetime;
			puddle.Launcher = launcher;
		}

		if ( trailSettings.Gas.Enabled )
		{
			var gas = zoneGo.Components.GetOrCreate<GasCloudDamage>();
			gas.DamagePerTick = trailSettings.Gas.DamagePerTick;
			gas.TickInterval = trailSettings.Gas.TickInterval;
			gas.Radius = trailSettings.Gas.Radius;
			gas.Lifetime = trailSettings.Gas.Lifetime;
			gas.Launcher = launcher;
		}
	}
}

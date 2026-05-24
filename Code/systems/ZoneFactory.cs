using Sandbox;
using MagicSystem;

public static class ZoneFactory
{
	private delegate void ProjectileApplier( GameObject zone, AttackProjectile config, GameObject launcher );
	private delegate void TrailApplier( GameObject zone, TrailSettings config, GameObject launcher );

	private static readonly List<ProjectileApplier> _projectileAppliers = new();
	private static readonly List<TrailApplier> _trailAppliers = new();

	static ZoneFactory()
	{
		// Explosion (projectile only — no trail equivalent)
		_projectileAppliers.Add( ( zone, c, l ) =>
		{
			if ( !c.Explosion.Enabled ) return;
			var exp = zone.Components.GetOrCreate<AoEExplosionDamage>();
			exp.Damage = c.Explosion.Damage;
			exp.Radius = c.Explosion.Radius;
			exp.ExplosionDebugLifetime = c.Explosion.DebugTime;
			exp.Launcher = l;
			exp.Explode();
		} );

		// Puddle
		_projectileAppliers.Add( ( zone, c, l ) => ApplyPuddle( zone, c.Puddle, l ) );
		_trailAppliers.Add( ( zone, t, l ) => ApplyPuddle( zone, t.Puddle, l ) );

		// Gas
		_projectileAppliers.Add( ( zone, c, l ) => ApplyGas( zone, c.Gas, l ) );
		_trailAppliers.Add( ( zone, t, l ) => ApplyGas( zone, t.Gas, l ) );
	}

	public static void ConfigureZoneEffects( GameObject zoneGo, AttackProjectile config, GameObject launcher )
	{
		foreach ( var apply in _projectileAppliers )
			apply( zoneGo, config, launcher );
	}

	public static void ConfigureZoneEffects( GameObject zoneGo, TrailSettings trailSettings, GameObject launcher )
	{
		foreach ( var apply in _trailAppliers )
			apply( zoneGo, trailSettings, launcher );
	}

	private static void ApplyPuddle( GameObject zone, PuddleSettings s, GameObject launcher )
	{
		if ( !s.Enabled ) return;
		var puddle = zone.Components.GetOrCreate<FirePuddleDamage>();
		puddle.DamagePerTick = s.DamagePerTick;
		puddle.TickInterval = s.TickInterval;
		puddle.Radius = s.Radius;
		puddle.MaxHeight = s.PuddleHeight;
		puddle.Lifetime = s.Lifetime;
		puddle.Launcher = launcher;
	}

	private static void ApplyGas( GameObject zone, GasSettings s, GameObject launcher )
	{
		if ( !s.Enabled ) return;
		var gas = zone.Components.GetOrCreate<GasCloudDamage>();
		gas.DamagePerTick = s.DamagePerTick;
		gas.TickInterval = s.TickInterval;
		gas.Radius = s.Radius;
		gas.Lifetime = s.Lifetime;
		gas.Launcher = launcher;
	}
}

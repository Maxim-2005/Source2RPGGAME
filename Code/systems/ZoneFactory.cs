using MagicSystem;

public static class ZoneFactory
{
	public static void ConfigureZoneEffects( GameObject zoneGo, AttackProjectile config, GameObject launcher )
	{
		ZoneEffectRegistry.ApplyProjectile( zoneGo, config, launcher );
	}

	public static void ConfigureZoneEffects( GameObject zoneGo, TrailSettings trailSettings, GameObject launcher )
	{
		ZoneEffectRegistry.ApplyTrail( zoneGo, trailSettings, launcher );
	}
}

using MagicSystem;

public sealed class GasZoneEffect : IZoneEffect
{
    public string Id => "gas";

    public void ApplyFromProjectile( GameObject zone, AttackProjectile config, GameObject launcher )
    {
        Apply( zone, config.Gas, launcher );
    }

    public void ApplyFromTrail( GameObject zone, TrailSettings config, GameObject launcher )
    {
        Apply( zone, config.Gas, launcher );
    }

    private static void Apply( GameObject zone, GasSettings s, GameObject launcher )
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

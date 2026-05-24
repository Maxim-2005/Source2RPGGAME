using MagicSystem;

public sealed class PuddleZoneEffect : IZoneEffect
{
    public string Id => "puddle";

    public void ApplyFromProjectile( GameObject zone, AttackProjectile config, GameObject launcher )
    {
        Apply( zone, config.Puddle, launcher );
    }

    public void ApplyFromTrail( GameObject zone, TrailSettings config, GameObject launcher )
    {
        Apply( zone, config.Puddle, launcher );
    }

    private static void Apply( GameObject zone, PuddleSettings s, GameObject launcher )
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
}

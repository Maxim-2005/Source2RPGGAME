using MagicSystem;

public sealed class ExplosionZoneEffect : IZoneEffect
{
    public string Id => "explosion";

    public void ApplyFromProjectile( GameObject zone, AttackProjectile config, GameObject launcher )
    {
        if ( !config.Explosion.Enabled ) return;
        var exp = zone.Components.GetOrCreate<AoEExplosionDamage>();
        exp.Damage = config.Explosion.Damage;
        exp.Radius = config.Explosion.Radius;
        exp.ExplosionDebugLifetime = config.Explosion.DebugTime;
        exp.Launcher = launcher;
        exp.Explode();
    }

    public void ApplyFromTrail( GameObject zone, TrailSettings config, GameObject launcher )
    {
    }
}

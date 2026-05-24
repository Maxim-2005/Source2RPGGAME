using System.Collections.Generic;
using MagicSystem;

public static class ZoneEffectRegistry
{
    private static bool _initialized;
    private static List<IZoneEffect> _effects;

    private static void EnsureInitialized()
    {
        if ( _initialized ) return;
        _effects = new List<IZoneEffect>();
        _effects.Add( new ExplosionZoneEffect() );
        _effects.Add( new PuddleZoneEffect() );
        _effects.Add( new GasZoneEffect() );
        _initialized = true;
    }

    public static void ApplyProjectile( GameObject zone, AttackProjectile config, GameObject launcher )
    {
        EnsureInitialized();
        foreach ( var effect in _effects )
            effect.ApplyFromProjectile( zone, config, launcher );
    }

    public static void ApplyTrail( GameObject zone, TrailSettings config, GameObject launcher )
    {
        EnsureInitialized();
        foreach ( var effect in _effects )
            effect.ApplyFromTrail( zone, config, launcher );
    }
}

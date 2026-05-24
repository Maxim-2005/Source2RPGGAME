using System;
using System.Collections.Generic;

public static class StatusEffectRegistry
{
    private static bool _initialized;
    private static Dictionary<string, Func<IStatusEffect>> _factories;

    private static void EnsureInitialized()
    {
        if ( _initialized ) return;
        _factories = new Dictionary<string, Func<IStatusEffect>>();
        _factories["burn"] = () => new BurnEffect();
        _factories["shield"] = () => new ShieldEffect();
        _initialized = true;
    }

    public static void Register( string id, Func<IStatusEffect> factory )
    {
        EnsureInitialized();
        _factories[id] = factory;
    }

    public static IStatusEffect Create( string id )
    {
        EnsureInitialized();
        return _factories.TryGetValue( id, out var factory ) ? factory() : null;
    }
}

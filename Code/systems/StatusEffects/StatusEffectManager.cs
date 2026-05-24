using Sandbox;
using System;
using System.Collections.Generic;

public sealed class StatusEffectManager : Component
{
    private readonly List<ActiveEffect> _activeEffects = new();
    private readonly List<ActiveEffect> _pendingRemoval = new();

    public IReadOnlyList<ActiveEffect> ActiveEffects => _activeEffects;

    protected override void OnStart()
    {
        DamageService.DamageModifiers.Add( ModifyDamage );
    }

    protected override void OnDestroy()
    {
        DamageService.DamageModifiers.Remove( ModifyDamage );
        ClearAll();
    }

    protected override void OnUpdate()
    {
        float delta = Time.Delta;
        _pendingRemoval.Clear();

        foreach ( var effect in _activeEffects )
        {
            effect.TimeRemaining -= delta;

            if ( effect.TimeRemaining <= 0f )
            {
                effect.Logic.OnRemoved( GameObject );
                _pendingRemoval.Add( effect );
                continue;
            }

            effect.TimeSinceLastTick += delta;
            if ( effect.TickInterval > 0f && effect.TimeSinceLastTick >= effect.TickInterval )
            {
                effect.TimeSinceLastTick = 0;
                effect.Logic.OnTick( GameObject, effect.Magnitude, effect.Source );
            }
        }

        foreach ( var effect in _pendingRemoval )
            _activeEffects.Remove( effect );
    }

    public void Apply( string id, float duration, float magnitude, float tickInterval, GameObject source )
    {
        var existing = _activeEffects.Find( e => e.Id == id );
        if ( existing != null )
        {
            existing.Logic.OnRemoved( GameObject );
            _activeEffects.Remove( existing );
        }

        var logic = StatusEffectRegistry.Create( id );
        if ( logic == null ) return;

        var effect = new ActiveEffect
        {
            Id = id,
            TimeRemaining = duration,
            Magnitude = magnitude,
            TickInterval = tickInterval,
            Source = source,
            Logic = logic,
            TimeSinceLastTick = 0
        };

        _activeEffects.Add( effect );
        logic.OnApplied( GameObject, magnitude );
    }

    public void Remove( string id )
    {
        var effect = _activeEffects.Find( e => e.Id == id );
        if ( effect == null ) return;
        effect.Logic.OnRemoved( GameObject );
        _activeEffects.Remove( effect );
    }

    public bool Has( string id )
    {
        return _activeEffects.Find( e => e.Id == id ) != null;
    }

    private float ModifyDamage( GameObject target, float damage, GameObject attacker )
    {
        float result = damage;
        foreach ( var effect in _activeEffects )
            result = effect.Logic.ModifyDamage( result, target, attacker );
        return result;
    }

    public static void TryApply( GameObject target, AttackProjectile config, GameObject launcher )
    {
        if ( config?.Spell?.StatusEffects == null || config.Spell.StatusEffects.Length == 0 )
            return;

        var manager = target.Components.Get<StatusEffectManager>();
        if ( manager == null )
            manager = target.Components.Create<StatusEffectManager>();

        foreach ( var se in config.Spell.StatusEffects )
            manager.Apply( se.Id, se.Duration, se.Magnitude, se.TickInterval, launcher );
    }

    private void ClearAll()
    {
        foreach ( var effect in _activeEffects )
            effect.Logic.OnRemoved( GameObject );
        _activeEffects.Clear();
    }
}

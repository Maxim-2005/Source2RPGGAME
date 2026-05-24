using System;
using Sandbox;

public sealed class ShieldEffect : IStatusEffect
{
    public string Id => "shield";

    private float _remainingShield;

    public void OnApplied( GameObject target, float magnitude )
    {
        _remainingShield = magnitude;
    }

    public void OnRemoved( GameObject target )
    {
    }

    public void OnTick( GameObject target, float magnitude, GameObject source )
    {
    }

    public float ModifyDamage( float incoming, GameObject target, GameObject attacker )
    {
        if ( _remainingShield <= 0f ) return incoming;

        float absorbed = Math.Min( _remainingShield, incoming );
        _remainingShield -= absorbed;
        return incoming - absorbed;
    }
}

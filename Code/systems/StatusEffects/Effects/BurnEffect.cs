using Sandbox;

public sealed class BurnEffect : IStatusEffect
{
    public string Id => "burn";

    public void OnApplied( GameObject target, float magnitude )
    {
    }

    public void OnRemoved( GameObject target )
    {
    }

    public void OnTick( GameObject target, float magnitude, GameObject source )
    {
        DamageService.ApplyDamage( target, magnitude, source );
    }

    public float ModifyDamage( float incoming, GameObject target, GameObject attacker )
    {
        return incoming;
    }
}

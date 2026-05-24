using Sandbox;

public interface IStatusEffect
{
    string Id { get; }
    void OnApplied( GameObject target, float magnitude );
    void OnRemoved( GameObject target );
    void OnTick( GameObject target, float magnitude, GameObject source );
    float ModifyDamage( float incoming, GameObject target, GameObject attacker );
}

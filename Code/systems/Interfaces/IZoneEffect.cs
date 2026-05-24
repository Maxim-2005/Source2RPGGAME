using Sandbox;
using MagicSystem;

public interface IZoneEffect
{
    string Id { get; }
    void ApplyFromProjectile( GameObject zone, AttackProjectile config, GameObject launcher );
    void ApplyFromTrail( GameObject zone, TrailSettings config, GameObject launcher );
}

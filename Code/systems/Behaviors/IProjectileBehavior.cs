using Sandbox;

public interface IProjectileBehavior
{
    void SpawnFrom( AttackProjectile attack, GameObject origin, Vector3 direction, float flightDistance, GameObject attacker );
    void Launch( MagicProjectile projectile, GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance );
    bool TryHandleCollision( MagicProjectile projectile, Vector3 nextPosition, out PhysicsTraceResult hitResult );
    void HandleMaxDistance( MagicProjectile projectile );
    void OnImpact( MagicProjectile projectile, PhysicsTraceResult tr );
    bool IsAreaTarget { get; }
}

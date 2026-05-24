using Sandbox;

public sealed class MeteorTracerBehavior : IProjectileBehavior
{
    private const float TracerSpeed = 150000f;
    private const float SpawnHeightOffsetFactor = 0.4f;

    public bool IsAreaTarget => true;

    public void SpawnFrom( AttackProjectile attack, GameObject origin, Vector3 direction, float flightDistance, GameObject attacker )
    {
        if ( attack.ProjectilePrefab == null ) return;
        var go = attack.ProjectilePrefab.Clone( origin.WorldPosition, Rotation.LookAt( direction ) );
        var projectile = go.Components.Get<MagicProjectile>();
        projectile?.Launch( this, attacker, direction, attack, flightDistance );
    }

    public void Launch( MagicProjectile projectile, GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance )
    {
        projectile.InternalLaunch( launcher, direction, config, TracerSpeed, flightDistance );

        var visuals = projectile.GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
        visuals?.HideAll();
    }

    public bool TryHandleCollision( MagicProjectile projectile, Vector3 nextPosition, out PhysicsTraceResult hitResult )
    {
        hitResult = default;
        return false;
    }

    public void HandleMaxDistance( MagicProjectile projectile )
    {
        Vector3 targetFloorPos = projectile.GameObject.WorldPosition;
        Vector3 skySpawnPos = targetFloorPos + ( Vector3.Up * projectile.Config.MeteorMode.SpawnHeight );
        skySpawnPos -= projectile.Direction * ( projectile.Config.MeteorMode.SpawnHeight * SpawnHeightOffsetFactor );
        Vector3 fallDirection = ( targetFloorPos - skySpawnPos ).Normal;

        var meteorGo = projectile.Config.ProjectilePrefab.Clone( skySpawnPos, Rotation.LookAt( fallDirection ) );
        var meteorScript = meteorGo.Components.Get<MagicProjectile>();
        var meteorBehavior = new MeteorBehavior();
        meteorScript?.Launch( meteorBehavior, projectile.Launcher, fallDirection, projectile.Config, null );

        projectile.GameObject.Destroy();
    }

    public void OnImpact( MagicProjectile projectile, PhysicsTraceResult tr )
    {
        Vector3 impactPos = tr.EndPosition;

        Vector3 skySpawnPos = impactPos + ( Vector3.Up * projectile.Config.MeteorMode.SpawnHeight );
        skySpawnPos -= projectile.Direction * ( projectile.Config.MeteorMode.SpawnHeight * SpawnHeightOffsetFactor );

        Vector3 fallDirection = ( impactPos - skySpawnPos ).Normal;
        var meteorGo = projectile.Config.ProjectilePrefab.Clone( skySpawnPos, Rotation.LookAt( fallDirection ) );
        var meteorScript = meteorGo.Components.Get<MagicProjectile>();
        var meteorBehavior = new MeteorBehavior();
        meteorScript?.Launch( meteorBehavior, projectile.Launcher, fallDirection, projectile.Config, null );

        projectile.GameObject.Destroy();
    }
}

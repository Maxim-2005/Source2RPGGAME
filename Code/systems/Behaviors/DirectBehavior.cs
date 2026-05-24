using Sandbox;

public sealed class DirectBehavior : IProjectileBehavior
{
    private const float SpawnOffset = 40f;

    public bool IsAreaTarget => false;

    public void SpawnFrom( AttackProjectile attack, GameObject origin, Vector3 direction, float flightDistance, GameObject attacker )
    {
        if ( attack.ProjectilePrefab == null ) return;
        Vector3 spawnPos = origin.WorldPosition + ( direction * SpawnOffset );
        var go = attack.ProjectilePrefab.Clone( spawnPos, Rotation.LookAt( direction ) );
        var projectile = go.Components.Get<MagicProjectile>();
        projectile?.Launch( this, attacker, direction, attack, flightDistance );
    }

    public void Launch( MagicProjectile projectile, GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance )
    {
        projectile.InternalLaunch( launcher, direction, config, config.DirectMode.Speed, flightDistance );

        var visuals = projectile.GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
        if ( visuals != null )
        {
            if ( visuals.DefaultBallVisual != null ) visuals.DefaultBallVisual.Enabled = true;
            if ( visuals.MeteorVisual != null ) visuals.MeteorVisual.Enabled = false;
            visuals.GameObject.WorldScale = new Vector3( config.DirectMode.ProjectileScale );
        }
    }

    public bool TryHandleCollision( MagicProjectile projectile, Vector3 nextPosition, out PhysicsTraceResult hitResult )
    {
        var tr = projectile.Scene.PhysicsWorld.Trace
            .Ray( projectile.GameObject.WorldPosition, nextPosition )
            .WithoutTags( GameTags.Projectile, GameTags.Trigger )
            .Run();
        hitResult = tr;
        return tr.Hit;
    }

    public void HandleMaxDistance( MagicProjectile projectile )
    {
        projectile.TriggerAirExplosion();
    }

    public void OnImpact( MagicProjectile projectile, PhysicsTraceResult tr )
    {
        GameObject hitTarget = tr.Body?.GameObject;
        if ( hitTarget.IsOwnedBy( projectile.Launcher ) ) return;

        if ( hitTarget != null && hitTarget.Tags.Has( GameTags.Enemy ) )
        {
            DamageService.ApplyDamage( hitTarget, projectile.Config.DirectMode.Damage, projectile.Launcher );
        }

        projectile.SpawnZoneEffects( tr.EndPosition );
        projectile.GameObject.Destroy();
    }
}

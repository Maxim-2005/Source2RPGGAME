using Sandbox;
using System.Collections.Generic;
using System;

public sealed class MeteorBehavior : IProjectileBehavior
{
    private const float MeteorRadiusBase = 16f;

    private readonly HashSet<Guid> _hitTargetsThisSpawn = new();

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
        projectile.InternalLaunch( launcher, direction, config, config.MeteorMode.FallSpeed, flightDistance );

        var visuals = projectile.GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
        if ( visuals != null )
        {
            if ( visuals.DefaultBallVisual != null ) visuals.DefaultBallVisual.Enabled = false;
            if ( visuals.MeteorVisual != null ) visuals.MeteorVisual.Enabled = true;
            visuals.GameObject.WorldScale = new Vector3( config.MeteorMode.Scale );
        }
    }

    public bool TryHandleCollision( MagicProjectile projectile, Vector3 nextPosition, out PhysicsTraceResult hitResult )
    {
        float radius = MeteorRadiusBase * projectile.Config.MeteorMode.Scale;
        var tr = projectile.Scene.PhysicsWorld.Trace
            .Sphere( radius, projectile.GameObject.WorldPosition, nextPosition )
            .WithoutTags( GameTags.Projectile, GameTags.Trigger )
            .Run();

        if ( tr.Hit && tr.Body?.GameObject != null )
        {
            var hitGo = tr.Body.GameObject;

            if ( hitGo.IsOwnedBy( projectile.Launcher ) )
            {
                tr.Hit = false;
            }
            else if ( hitGo.Tags.Has( GameTags.Player ) || hitGo.Tags.Has( GameTags.Enemy ) )
            {
                if ( projectile.Config.MeteorMode.HasDirectHit && hitGo.Tags.Has( GameTags.Enemy ) )
                {
                    ApplyDirectDamage( projectile, hitGo );
                }
                tr.Hit = false;
            }
        }

        hitResult = tr;
        return tr.Hit;
    }

    private void ApplyDirectDamage( MagicProjectile projectile, GameObject target )
    {
        if ( _hitTargetsThisSpawn.Contains( target.Id ) ) return;
        _hitTargetsThisSpawn.Add( target.Id );
        DamageService.ApplyDamage( target, projectile.Config.MeteorMode.Damage, projectile.Launcher );
    }

    public void HandleMaxDistance( MagicProjectile projectile )
    {
        projectile.TriggerAirExplosion();
    }

    public void OnImpact( MagicProjectile projectile, PhysicsTraceResult tr )
    {
        Vector3 impactPos = tr.EndPosition;

        if ( ShouldSpawnRollingBoulder( projectile ) )
        {
            SpawnRollingBoulder( projectile, impactPos );
        }

        projectile.SpawnZoneEffects( impactPos );
        projectile.GameObject.Destroy();
    }

    private static bool ShouldSpawnRollingBoulder( MagicProjectile projectile )
    {
        return projectile.Config.MeteorMode.RollAfterImpact
            && projectile.Config.MeteorMode.RollingPrefab != null;
    }

    private void SpawnRollingBoulder( MagicProjectile projectile, Vector3 impactPosition )
    {
        var currentVisuals = projectile.GameObject.Components.Get<ProjectileVisuals>( FindMode.EverythingInSelfAndChildren );
        currentVisuals?.HideAll();

        var rootRenderer = projectile.GameObject.Components.Get<ModelRenderer>();
        if ( rootRenderer != null ) rootRenderer.Enabled = false;

        Vector3 rollDir = new Vector3( projectile.Direction.x, projectile.Direction.y, 0 ).Normal;

        var rollingGo = projectile.Config.MeteorMode.RollingPrefab.Clone( impactPosition, Rotation.Identity );
        rollingGo.WorldScale = projectile.Config.MeteorMode.Scale;

        var rollingScript = rollingGo.Components.Get<MeteorRollingLogic>();
        rollingScript?.InitializeRoll( projectile.Launcher, rollDir, projectile.Config, MeteorRadiusBase );

        var spawnerScript = rollingGo.Components.Get<ObjectTrailSpawner>();
        if ( spawnerScript != null )
        {
            spawnerScript.SpawnInterval = projectile.Config.MeteorMode.TrailSpawnInterval;
        }
    }
}

using Sandbox;
using System.Collections.Generic;
using MagicSystem;

public sealed class ChainLightningBehavior : IProjectileBehavior
{
    private const float DebugLineDuration = 1.5f;

    public bool IsAreaTarget => false;

    public void SpawnFrom( AttackProjectile attack, GameObject origin, Vector3 direction, float flightDistance, GameObject attacker )
    {
        if ( attack == null || origin == null || attacker == null ) return;

        var tr = attack.Scene.PhysicsWorld.Trace
            .Ray( origin.WorldPosition, origin.WorldPosition + direction * flightDistance )
            .WithoutTags( GameTags.Player, GameTags.Projectile, GameTags.Trigger )
            .Run();

        if ( !tr.Hit ) return;

        GameObject firstTarget = tr.Body?.GameObject;
        if ( firstTarget == null ) return;
        if ( firstTarget.IsOwnedBy( attacker ) ) return;
        if ( !firstTarget.Tags.Has( GameTags.Enemy ) ) return;

        var settings = attack.ChainLightningMode;

        DamageService.ApplyDamage( firstTarget, settings.Damage, attacker );
        StatusEffectManager.TryApply( firstTarget, attack, attacker );

        var lines = new List<DebugChainVisualizer.LineData>
        {
            new() { Start = origin.WorldPosition, End = firstTarget.WorldPosition }
        };

        ChainToNearby( attack.Scene, firstTarget, attacker, settings, lines );

        var debugGo = new GameObject();
        var viz = debugGo.AddComponent<DebugChainVisualizer>();
        viz.Lines = lines.ToArray();
        viz.Lifetime = DebugLineDuration;
    }

    public void Launch( MagicProjectile projectile, GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance )
    {
    }

    public bool TryHandleCollision( MagicProjectile projectile, Vector3 nextPosition, out PhysicsTraceResult hitResult )
    {
        hitResult = default;
        return false;
    }

    public void HandleMaxDistance( MagicProjectile projectile )
    {
    }

    public void OnImpact( MagicProjectile projectile, PhysicsTraceResult tr )
    {
    }

    private static void ChainToNearby( Scene scene, GameObject originTarget, GameObject launcher, ChainLightningSettings settings, List<DebugChainVisualizer.LineData> lines )
    {
        var hitIds = new HashSet<GameObject> { originTarget };
        GameObject current = originTarget;
        float damage = settings.Damage;

        for ( int i = 1; i < settings.MaxTargets; i++ )
        {
            damage *= settings.DamageFalloff;
            if ( damage <= 0f ) break;

            var allHealth = scene.GetAllComponents<HealthComponent>();
            GameObject nearest = null;
            float nearestDist = float.MaxValue;
            Vector3 originPos = current.WorldPosition;

            foreach ( var hc in allHealth )
            {
                var go = hc.GameObject;
                if ( go == null || !go.IsValid() ) continue;
                if ( hitIds.Contains( go ) ) continue;
                if ( go.IsOwnedBy( launcher ) ) continue;
                if ( !go.Tags.Has( GameTags.Enemy ) ) continue;

                float dist = Vector3.DistanceBetween( originPos, go.WorldPosition );
                if ( dist < nearestDist && dist <= settings.ChainRadius )
                {
                    nearest = go;
                    nearestDist = dist;
                }
            }

            if ( nearest == null ) break;

            lines.Add( new DebugChainVisualizer.LineData { Start = current.WorldPosition, End = nearest.WorldPosition } );
            DamageService.ApplyDamage( nearest, damage, launcher );
            hitIds.Add( nearest );
            current = nearest;
        }
    }
}

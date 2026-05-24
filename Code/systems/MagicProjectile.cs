using Sandbox;
using System;

public sealed class MagicProjectile : Component
{
    [Hide] private IProjectileBehavior _behavior = null!;
    [Hide] private Vector3 _direction;
    [Hide] private GameObject _launcher;
    [Hide] private AttackProjectile _config;
    [Hide] private bool _launched;
    [Hide] private float _currentSpeed = 2500f;
    [Hide] private Vector3 _startPosition;
    [Hide] private float? _maxFlightDistance;

    public Vector3 Direction => _direction;
    public GameObject Launcher => _launcher;
    public AttackProjectile Config => _config;

    public void Launch( IProjectileBehavior behavior, GameObject launcher, Vector3 direction, AttackProjectile config, float? flightDistance )
    {
        _behavior = behavior;
        behavior.Launch( this, launcher, direction, config, flightDistance );
    }

    public void InternalLaunch( GameObject launcher, Vector3 direction, AttackProjectile config, float speed, float? flightDistance )
    {
        _direction = direction.Normal;
        _launcher = launcher;
        _config = config;
        _currentSpeed = speed;
        _startPosition = GameObject.WorldPosition;
        _maxFlightDistance = flightDistance;
        _launched = true;
    }

    protected override void OnUpdate()
    {
        if ( !_launched || _config == null ) return;

        float travelDistance = Vector3.DistanceBetween( _startPosition, GameObject.WorldPosition );

        if ( _maxFlightDistance.HasValue && travelDistance >= _maxFlightDistance.Value )
        {
            _behavior.HandleMaxDistance( this );
            return;
        }

        Vector3 currentPos = GameObject.WorldPosition;
        float step = _currentSpeed * Time.Delta;

        if ( _maxFlightDistance.HasValue )
        {
            float remaining = _maxFlightDistance.Value - travelDistance;
            if ( step > remaining )
                step = Math.Max( remaining, 0f );
        }

        if ( step <= 0f )
        {
            _behavior.HandleMaxDistance( this );
            return;
        }

        Vector3 nextPosition = currentPos + _direction * step;

        if ( _behavior.TryHandleCollision( this, nextPosition, out var hitResult ) )
        {
            _behavior.OnImpact( this, hitResult );
            return;
        }

        GameObject.WorldPosition = nextPosition;
    }

    public void TriggerAirExplosion()
    {
        _behavior.OnImpact( this, new PhysicsTraceResult
        {
            Hit = true,
            EndPosition = GameObject.WorldPosition,
            Normal = -_direction
        } );
    }

    public void SpawnZoneEffects( Vector3 position )
    {
        if ( _config.ZonePrefab == null ) return;
        if ( !_config.Explosion.Enabled && !_config.Puddle.Enabled && !_config.Gas.Enabled ) return;

        var zoneGo = _config.ZonePrefab.Clone( position, Rotation.Identity );
        ZoneFactory.ConfigureZoneEffects( zoneGo, _config, _launcher );
    }
}

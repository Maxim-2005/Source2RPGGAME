using System;
using System.Threading.Tasks;
using Sandbox;
using MagicSystem;

public sealed class AttackProjectile : BaseAttackModule, IAreaRadiusProvider
{
	[Property, Group( "Spawn" )] public GameObject ProjectilePrefab { get; set; }
	[Property, Group( "Spawn" )] public GameObject LaunchPoint { get; set; }
	[Property, Group( "Spawn" )] public GameObject ZonePrefab { get; set; }

	[Property, Group( "Timings" )] public float PreAttackDelay { get; set; } = 0.15f;
	[Property, Group( "Timings" )] public float AttackCooldown { get; set; } = 0.8f;

	[Property, Group( "Core Logic" )] public ProjectileType MagicType { get; set; } = ProjectileType.Direct;
	[Property, Group( "Core Logic" )] public float MaxRange { get; set; } = 2500f;

	[Property, Group( "Configurations" )] public DirectSettings DirectMode { get; set; } = new();
	[Property, Group( "Configurations" )] public MeteorSettings MeteorMode { get; set; } = new();
	[Property, Group( "Configurations" )] public ExplosionSettings Explosion { get; set; } = new();
	[Property, Group( "Configurations" )] public PuddleSettings Puddle { get; set; } = new();
	[Property, Group( "Configurations" )] public GasSettings Gas { get; set; } = new();

	public float GetMaxAreaRadius()
	{
		float maxRadius = 0f;

		if ( Explosion.Enabled )
			maxRadius = Math.Max( maxRadius, Explosion.Radius );
		if ( Puddle.Enabled )
			maxRadius = Math.Max( maxRadius, Puddle.Radius );
		if ( Gas.Enabled )
			maxRadius = Math.Max( maxRadius, Gas.Radius );

		return maxRadius > 0f ? maxRadius : 150f;
	}

	private TimeSince _timeSinceLastAttack = 100f;

	public override bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		if ( _timeSinceLastAttack < AttackCooldown || IsAttacking ) return false;

		_ = ProcessShoot( attacker, playerModel );
		return true;
	}

	public override void StopAttack() { }

	private async Task ProcessShoot( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		IsAttacking = true;
		_timeSinceLastAttack = 0;

		if ( playerModel != null ) playerModel.Set( "b_attack", true );
		await Task.DelaySeconds( PreAttackDelay );

		if ( !IsValid || !GameObject.IsValid() || GameObject.Parent == null )
		{
			IsAttacking = false;
			return;
		}

		GameObject origin = LaunchPoint != null ? LaunchPoint : GameObject;
		Vector3 shootDirection;
		float flightDistance = MaxRange;

		if ( attacker != null )
		{
			var camera = attacker.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndDescendants );
			if ( camera != null )
			{
				Vector3 traceStart = camera.WorldPosition;
				Vector3 traceEnd = camera.WorldPosition + camera.WorldRotation.Forward * 10000f;

				var cameraTrace = Scene.PhysicsWorld.Trace
					.Ray( traceStart, traceEnd )
					.WithoutTags( "player", "projectile", "trigger" )
					.Run();

				Vector3 rawTarget = cameraTrace.Hit
					? cameraTrace.EndPosition
					: traceEnd;

				Vector3 toOrigin = rawTarget - origin.WorldPosition;
				float originDist = toOrigin.Length;
				Vector3 finalTarget = originDist <= MaxRange
					? rawTarget
					: origin.WorldPosition + toOrigin.Normal * MaxRange;

				if ( !cameraTrace.Hit )
				{
					var groundTrace = Scene.PhysicsWorld.Trace
						.Ray( finalTarget, finalTarget + Vector3.Down * 5000f )
						.WithoutTags( "player", "projectile", "trigger" )
						.Run();

					if ( groundTrace.Hit )
						finalTarget = groundTrace.EndPosition;
				}

				Vector3 toTarget = finalTarget - origin.WorldPosition;
				if ( toTarget.Length < 0.1f || Vector3.Dot( toTarget.Normal, camera.WorldRotation.Forward ) <= 0f )
				{
					shootDirection = camera.WorldRotation.Forward;
				}
				else
				{
					shootDirection = toTarget.Normal;
					flightDistance = toTarget.Length;
				}
			}
			else
			{
				shootDirection = origin.WorldRotation.Forward;
			}
		}
		else
		{
			shootDirection = origin.WorldRotation.Forward;
		}

		shootDirection = shootDirection.Normal;

		if ( ProjectilePrefab != null )
		{
			if ( MagicType == ProjectileType.Direct )
			{
				Vector3 finalSpawnPos = origin.WorldPosition + (shootDirection * 40f);
				var projectileGo = ProjectilePrefab.Clone( finalSpawnPos, Rotation.LookAt( shootDirection ) );
				var projectileScript = projectileGo.Components.Get<MagicProjectile>();

				projectileScript?.LaunchAsDirect( attacker, shootDirection, this, DirectMode.ProjectileScale, flightDistance: flightDistance );
			}
			else if ( MagicType == ProjectileType.Meteor )
			{
				var projectileGo = ProjectilePrefab.Clone( origin.WorldPosition, Rotation.LookAt( shootDirection ) );
				var projectileScript = projectileGo.Components.Get<MagicProjectile>();
				projectileScript?.LaunchAsMeteorTracer( attacker, shootDirection, this, flightDistance );
			}
		}

		IsAttacking = false;
	}
}

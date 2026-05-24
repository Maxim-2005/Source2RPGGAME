using System;
using System.Threading.Tasks;
using Sandbox;
using MagicSystem;

public sealed class AttackProjectile : BaseAttackModule, IAreaRadiusProvider
{
	[Property, Group( "Setup" )] public SpellDefinition Spell { get; set; }
	[Property, Group( "Spawn" )] public GameObject LaunchPoint { get; set; }

	public GameObject ProjectilePrefab => Spell?.ProjectilePrefab;
	public GameObject ZonePrefab => Spell?.ZonePrefab;
	public float PreAttackDelay => Spell?.PreAttackDelay ?? 0.15f;
	public float AttackCooldown => Spell?.AttackCooldown ?? 0.8f;
	public ProjectileType MagicType => Spell?.MagicType ?? ProjectileType.Direct;
	public float MaxRange => Spell?.MaxRange ?? 2500f;
	public DirectSettings DirectMode => Spell?.DirectMode ?? new();
	public MeteorSettings MeteorMode => Spell?.MeteorMode ?? new();
	public ExplosionSettings Explosion => Spell?.Explosion ?? new();
	public PuddleSettings Puddle => Spell?.Puddle ?? new();
	public GasSettings Gas => Spell?.Gas ?? new();

	private const float SpawnOffset = 40f;

	public float GetMaxAreaRadius() => Spell?.GetMaxAreaRadius() ?? 150f;

	[Hide] private TimeSince _timeSinceLastAttack = 100f;

	public override bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		if ( _timeSinceLastAttack < AttackCooldown || IsAttacking ) return false;

		_ = ProcessShoot( attacker, playerModel );
		return true;
	}

	public override void StopAttack() { }

	private async Task ProcessShoot( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		_timeSinceLastAttack = 0;
		await RunAttackAsync( PreAttackDelay, playerModel, "b_attack", async () =>
		{
			GameObject origin = LaunchPoint != null ? LaunchPoint : GameObject;
			Vector3 shootDirection;
			float flightDistance = MaxRange;

			if ( attacker != null )
			{
				var camera = attacker.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndDescendants );
				if ( camera != null )
				{
					var aim = AimHelper.Calculate( Scene, origin.WorldPosition, camera.WorldPosition, camera.WorldRotation, MaxRange );

					if ( aim.Distance < 0.1f || Vector3.Dot( aim.Direction, camera.WorldRotation.Forward ) <= 0f )
						shootDirection = camera.WorldRotation.Forward;
					else
					{
						shootDirection = aim.Direction;
						flightDistance = aim.Distance;
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
					Vector3 finalSpawnPos = origin.WorldPosition + (shootDirection * SpawnOffset);
					var projectileGo = ProjectilePrefab.Clone( finalSpawnPos, Rotation.LookAt( shootDirection ) );
					var projectileScript = projectileGo.Components.Get<MagicProjectile>();

					projectileScript?.LaunchAsDirect( attacker, shootDirection, this, flightDistance );
				}
				else if ( MagicType == ProjectileType.Meteor )
				{
					var projectileGo = ProjectilePrefab.Clone( origin.WorldPosition, Rotation.LookAt( shootDirection ) );
					var projectileScript = projectileGo.Components.Get<MagicProjectile>();
					projectileScript?.LaunchAsMeteorTracer( attacker, shootDirection, this, flightDistance );
				}
			}
		} );
	}
}

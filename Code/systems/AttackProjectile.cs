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
	public float MaxRange => Spell?.MaxRange ?? 2500f;
	public DirectSettings DirectMode => Spell?.DirectMode ?? new();
	public MeteorSettings MeteorMode => Spell?.MeteorMode ?? new();
	public ChainLightningSettings ChainLightningMode => Spell?.ChainLightningMode ?? new();
	public ExplosionSettings Explosion => Spell?.Explosion ?? new();
	public PuddleSettings Puddle => Spell?.Puddle ?? new();
	public GasSettings Gas => Spell?.Gas ?? new();

	public float GetMaxAreaRadius() => Spell?.GetMaxAreaRadius() ?? 150f;
	public IProjectileBehavior Behavior => Spell?.CreateBehavior();

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

			var behavior = Behavior;

			if ( attacker != null )
			{
				var camera = attacker.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndDescendants );
				if ( camera != null )
				{
					if ( behavior != null && behavior.IsAreaTarget )
					{
						var aim = AimHelper.Calculate( Scene, origin.WorldPosition, camera.WorldPosition, camera.WorldRotation, MaxRange );

						if ( aim.Distance >= 0.1f && Vector3.Dot( aim.Direction, camera.WorldRotation.Forward ) > 0f )
						{
							shootDirection = aim.Direction;
							flightDistance = aim.Distance;
						}
						else
						{
							shootDirection = camera.WorldRotation.Forward;
						}
					}
					else
					{
						shootDirection = camera.WorldRotation.Forward;
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

			if ( ProjectilePrefab == null ) return;

			behavior?.SpawnFrom( this, origin, shootDirection, flightDistance, attacker );
		} );
	}
}

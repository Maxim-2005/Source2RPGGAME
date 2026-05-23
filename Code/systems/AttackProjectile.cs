using System;
using System.Threading.Tasks;
using Sandbox;
using MagicSystem;

public sealed class AttackProjectile : BaseAttackModule
{
	[Property, Group( "Spawn" )] public GameObject ProjectilePrefab { get; set; }
	[Property, Group( "Spawn" )] public GameObject LaunchPoint { get; set; }
	[Property, Group( "Spawn" )] public GameObject ZonePrefab { get; set; }

	[Property, Group( "Timings" )] public float PreAttackDelay { get; set; } = 0.15f;
	[Property, Group( "Timings" )] public float AttackCooldown { get; set; } = 0.8f;

	[Property, Group( "Core Logic" )] public ProjectileType MagicType { get; set; } = ProjectileType.Direct;

	[Property, Group( "Configurations" )] public DirectSettings DirectMode { get; set; } = new();
	[Property, Group( "Configurations" )] public MeteorSettings MeteorMode { get; set; } = new();
	[Property, Group( "Configurations" )] public ExplosionSettings Explosion { get; set; } = new();
	[Property, Group( "Configurations" )] public PuddleSettings Puddle { get; set; } = new();
	[Property, Group( "Configurations" )] public GasSettings Gas { get; set; } = new();

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
		Vector3 shootDirection = attacker != null ?
			attacker.Components.Get<CameraComponent>( FindMode.EverythingInSelfAndDescendants )?.WorldRotation.Forward ?? origin.WorldRotation.Forward
			: origin.WorldRotation.Forward;

		shootDirection = shootDirection.Normal;

		if ( ProjectilePrefab != null )
		{
			if ( MagicType == ProjectileType.Direct )
			{
				Vector3 finalSpawnPos = origin.WorldPosition + (shootDirection * 40f);
				var projectileGo = ProjectilePrefab.Clone( finalSpawnPos, Rotation.LookAt( shootDirection ) );
				var projectileScript = projectileGo.Components.Get<MagicProjectile>();

				projectileScript?.LaunchAsDirect( attacker, shootDirection, this, DirectMode.ProjectileScale );
			}
			else if ( MagicType == ProjectileType.Meteor )
			{
				var projectileGo = ProjectilePrefab.Clone( origin.WorldPosition, Rotation.LookAt( shootDirection ) );
				var projectileScript = projectileGo.Components.Get<MagicProjectile>();
				projectileScript?.LaunchAsMeteorTracer( attacker, shootDirection, this );
			}
		}

		IsAttacking = false;
	}
}

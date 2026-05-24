using System;
using Sandbox;
using MagicSystem;

[AssetType( Name = "Spell Definition", Extension = "spell", Category = "Magic" )]
[Title( "Spell Definition" ), Description( "A magic spell or ability definition" )]
public class SpellDefinition : GameResource
{
	[Category( "Base" )]
	public string SpellName { get; set; } = "Fireball";
	public float ManaCost { get; set; } = 10f;
	public float AttackCooldown { get; set; } = 0.8f;
	public float PreAttackDelay { get; set; } = 0.15f;

	[Category( "Projectile" )]
	public ProjectileType MagicType { get; set; } = ProjectileType.Direct;
	public GameObject ProjectilePrefab { get; set; }
	public float MaxRange { get; set; } = 2500f;
	public DirectSettings DirectMode { get; set; } = new();
	public MeteorSettings MeteorMode { get; set; } = new();

	[Category( "Impact Effects" )]
	public GameObject ZonePrefab { get; set; }
	public ExplosionSettings Explosion { get; set; } = new();
	public PuddleSettings Puddle { get; set; } = new();
	public GasSettings Gas { get; set; } = new();

	[Category( "Status Effects" )]
	public StatusEffectApply[] StatusEffects { get; set; } = Array.Empty<StatusEffectApply>();

	public IProjectileBehavior CreateBehavior()
	{
		return MagicType switch
		{
			ProjectileType.Meteor => new MeteorTracerBehavior(),
			_ => new DirectBehavior()
		};
	}

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
}

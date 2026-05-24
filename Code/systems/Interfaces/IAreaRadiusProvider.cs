using MagicSystem;

public interface IAreaRadiusProvider
{
	ProjectileType MagicType { get; }
	bool IsAttacking { get; }
	float GetMaxAreaRadius();
	float MaxRange { get; }
}

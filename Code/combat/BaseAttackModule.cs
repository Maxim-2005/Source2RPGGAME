using Sandbox;

public abstract class BaseAttackModule : Component
{
	public bool IsAttacking { get; protected set; } = false;

	/// <summary>
	/// ѕытаетс€ совершить атаку. ¬озвращает true, если атака успешно началась.
	/// </summary>
	public abstract bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel );

	/// <summary>
	/// ѕринудительно останавливает атаку (например, если игрок отпустил зажим или сменил оружие).
	/// </summary>
	public abstract void StopAttack();
}

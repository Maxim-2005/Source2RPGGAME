using Sandbox;

public sealed class HealthComponent : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;
	public float CurrentHealth { get; private set; }

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void TakeDamage( float damageAmount, GameObject attacker )
	{
		if ( CurrentHealth <= 0 ) return;

		CurrentHealth -= damageAmount;
		Log.Info( $"{GameObject.Name} took {damageAmount} damage from {attacker.Name}. HP left: {CurrentHealth}" );

		if ( CurrentHealth <= 0 )
		{
			Die();
		}
	}

	private void Die()
	{
		Log.Info( $"{GameObject.Name} died!" );

		// All death-related things should be here: death sound, spawn death effects
		GameObject.Destroy();
	}
}

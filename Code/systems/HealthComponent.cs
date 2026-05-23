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
		Log.Info( $"{GameObject.Name} получил {damageAmount} урона от {attacker.Name}. Осталось ХП: {CurrentHealth}" );

		if ( CurrentHealth <= 0 )
		{
			Die();
		}
	}

	private void Die()
	{
		Log.Info( $"{GameObject.Name} погиб!" );

		// Здесь в будущем будет логика смерти: спавн лута, удаление объекта или запуск регдолла
		GameObject.Destroy();
	}
}

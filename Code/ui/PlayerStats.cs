public sealed class PlayerStats : Component
{
	private HealthComponent _health;

	[Property] public float MaxMana { get; set; } = 100f;
	[Property] public float MaxStamina { get; set; } = 100f;

	public float Health => _health?.CurrentHealth ?? 0f;
	public float MaxHealth => _health?.MaxHealth ?? 0f;
	public float Mana { get; private set; }
	public float Stamina { get; private set; }

	protected override void OnStart()
	{
		_health = Components.Get<HealthComponent>();
		Mana = MaxMana;
		Stamina = MaxStamina;
	}

	public bool TrySpendMana( float cost )
	{
		if ( Mana < cost ) return false;
		Mana -= cost;
		return true;
	}

	public void RestoreMana( float amount )
	{
		Mana = (Mana + amount).Clamp( 0f, MaxMana );
	}

	public bool TryDrainStamina( float amount )
	{
		if ( Stamina <= 0f ) return false;
		Stamina = (Stamina - amount).Clamp( 0f, MaxStamina );
		return Stamina > 0f;
	}

	public void RestoreStamina( float amount )
	{
		Stamina = (Stamina + amount).Clamp( 0f, MaxStamina );
	}
}

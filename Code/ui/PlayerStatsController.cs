public sealed class PlayerStatsController : Component
{
	[Property] public float ManaRegen { get; set; } = 10f;
	[Property] public float StaminaDrain { get; set; } = 25f;
	[Property] public float StaminaRegen { get; set; } = 15f;

	private PlayerStats _stats;

	protected override void OnStart()
	{
		_stats = Components.Get<PlayerStats>();
	}

	protected override void OnUpdate()
	{
		if ( _stats is null ) return;

		if ( _stats.Mana < _stats.MaxMana )
			_stats.RestoreMana( ManaRegen * Time.Delta );

		if ( Input.Down( "Run" ) )
			_stats.TryDrainStamina( StaminaDrain * Time.Delta );
		else if ( _stats.Stamina < _stats.MaxStamina )
			_stats.RestoreStamina( StaminaRegen * Time.Delta );
	}
}

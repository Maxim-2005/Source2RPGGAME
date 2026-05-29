using Sandbox.UI;

public sealed class PlayerHud : PanelComponent
{
	private PlayerStats _stats;
	private StatBar _healthBar;
	private StatBar _manaBar;
	private StatBar _staminaBar;

	protected override void OnStart()
	{
		_stats = Scene.GetAll<PlayerStats>().FirstOrDefault();

		Panel.Style.FlexDirection = FlexDirection.Column;
		Panel.Style.Position = PositionMode.Absolute;
		Panel.Style.Bottom = Length.Pixels( 40 );
		Panel.Style.Left = Length.Pixels( 40 );
		Panel.Style.Width = Length.Pixels( 280 );
		Panel.Style.Padding = Length.Pixels( 12 );
		Panel.Style.BackgroundColor = new Color( 0f, 0f, 0f, 0.2f );

		_healthBar = new StatBar
		{
			LabelText = "HP",
			BarColor = "#e74c3c",
			Parent = Panel
		};
		_manaBar = new StatBar
		{
			LabelText = "MP",
			BarColor = "#3498db",
			Parent = Panel
		};
		_staminaBar = new StatBar
		{
			LabelText = "SP",
			BarColor = "#2ecc71",
			Parent = Panel
		};
	}

	protected override void OnUpdate()
	{
		if ( _stats is null )
			return;

		_healthBar.Current = _stats.Health;
		_healthBar.Max = _stats.MaxHealth;
		_manaBar.Current = _stats.Mana;
		_manaBar.Max = _stats.MaxMana;
		_staminaBar.Current = _stats.Stamina;
		_staminaBar.Max = _stats.MaxStamina;
	}
}

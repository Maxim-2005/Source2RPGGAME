using Sandbox.UI;

public sealed class Crosshair : PanelComponent
{
	private Panel _center;
	private readonly Panel[] _arms = new Panel[4];
	private Panel _hitIndicator;
	private float _hitTimer;

	private const float ArmThickness = 2f;
	private const float ArmLength = 10f;
	private const float Gap = 6f;

	protected override void OnStart()
	{
		Panel.Style.Width = Length.Percent( 100 );
		Panel.Style.Height = Length.Percent( 100 );
		Panel.Style.Position = PositionMode.Relative;
		Panel.Style.PointerEvents = PointerEvents.None;

		_center = new Panel();
		_center.Parent = Panel;
		_center.Style.Position = PositionMode.Absolute;
		_center.Style.Left = Length.Percent( 50f );
		_center.Style.Top = Length.Percent( 50f );
		_center.Style.Width = Length.Pixels( 0f );
		_center.Style.Height = Length.Pixels( 0f );

		for ( var i = 0; i < 4; i++ )
		{
			_arms[i] = new Panel();
			_arms[i].Parent = _center;
			_arms[i].Style.Position = PositionMode.Absolute;
			_arms[i].Style.BackgroundColor = new Color( 1f, 1f, 1f, 0.7f );
		}

		_arms[0].Style.Left = Length.Pixels( -ArmThickness * 0.5f );
		_arms[0].Style.Top = Length.Pixels( -Gap - ArmLength );
		_arms[0].Style.Width = Length.Pixels( ArmThickness );
		_arms[0].Style.Height = Length.Pixels( ArmLength );

		_arms[1].Style.Left = Length.Pixels( -ArmThickness * 0.5f );
		_arms[1].Style.Top = Length.Pixels( Gap );
		_arms[1].Style.Width = Length.Pixels( ArmThickness );
		_arms[1].Style.Height = Length.Pixels( ArmLength );

		_arms[2].Style.Left = Length.Pixels( -Gap - ArmLength );
		_arms[2].Style.Top = Length.Pixels( -ArmThickness * 0.5f );
		_arms[2].Style.Width = Length.Pixels( ArmLength );
		_arms[2].Style.Height = Length.Pixels( ArmThickness );

		_arms[3].Style.Left = Length.Pixels( Gap );
		_arms[3].Style.Top = Length.Pixels( -ArmThickness * 0.5f );
		_arms[3].Style.Width = Length.Pixels( ArmLength );
		_arms[3].Style.Height = Length.Pixels( ArmThickness );

		_hitIndicator = new Panel();
		_hitIndicator.Parent = _center;
		_hitIndicator.Style.Position = PositionMode.Absolute;
		_hitIndicator.Style.Left = Length.Pixels( -6f );
		_hitIndicator.Style.Top = Length.Pixels( -6f );
		_hitIndicator.Style.Width = Length.Pixels( 12f );
		_hitIndicator.Style.Height = Length.Pixels( 12f );
		_hitIndicator.Style.Opacity = 0;
	}

	protected override void OnUpdate()
	{
		if ( _hitIndicator.Style.Opacity > 0f )
		{
			_hitTimer += Time.Delta;
			if ( _hitTimer >= 0.1f )
				_hitIndicator.Style.Opacity = 0;
		}
	}

	public void FlashHit( Color color )
	{
		_hitIndicator.Style.BackgroundColor = color;
		_hitIndicator.Style.Opacity = 1;
		_hitTimer = 0f;
	}

	public void HideHit()
	{
		_hitIndicator.Style.Opacity = 0;
	}
}

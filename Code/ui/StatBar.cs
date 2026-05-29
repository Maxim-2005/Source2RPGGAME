using Sandbox.UI;

public sealed class StatBar : Panel
{
	private readonly Panel _bg;
	private readonly Panel _fill;
	private readonly Label _label;

	public string LabelText { get; set; } = "HP";
	public float Current { get; set; } = 100f;
	public float Max { get; set; } = 100f;
	public string BarColor { get; set; } = "#e74c3c";

	public StatBar()
	{
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Pixels( 28 );
		Style.MarginBottom = Length.Pixels( 4 );
		Style.Position = PositionMode.Relative;

		_bg = new Panel();
		_bg.Parent = this;
		_bg.Style.Position = PositionMode.Absolute;
		_bg.Style.Left = 0;
		_bg.Style.Top = 0;
		_bg.Style.Width = Length.Percent( 100 );
		_bg.Style.Height = Length.Percent( 100 );
		_bg.Style.BackgroundColor = new Color( 0f, 0f, 0f, 0.35f );

		_fill = new Panel();
		_fill.Parent = this;
		_fill.Style.Position = PositionMode.Absolute;
		_fill.Style.Left = 0;
		_fill.Style.Top = 0;
		_fill.Style.Height = Length.Percent( 100 );
		_fill.Style.BackgroundColor = Color.Parse( BarColor ) ?? Color.Red;

		_label = new Label();
		_label.Parent = this;
		_label.Style.Position = PositionMode.Absolute;
		_label.Style.Left = Length.Pixels( 8 );
		_label.Style.Top = 0;
		_label.Style.Width = Length.Percent( 100 );
		_label.Style.Height = Length.Percent( 100 );
		_label.Style.FontColor = Color.White;
		_label.Style.FontSize = Length.Pixels( 14 );
		_label.Style.FontWeight = 700;
	}

	public override void Tick()
	{
		var pct = ( Max > 0f ) ? ( Current / Max * 100f ) : 0f;
		_fill.Style.Width = Length.Percent( pct );
		_fill.Style.BackgroundColor = Color.Parse( BarColor ) ?? Color.Red;
		_label.Text = $"{LabelText} {Current:F0}/{Max:F0}";
	}
}

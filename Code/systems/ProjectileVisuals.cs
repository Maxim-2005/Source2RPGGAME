using Sandbox;

public sealed class ProjectileVisuals : Component
{
	[Property] public GameObject DefaultBallVisual { get; set; }
	[Property] public GameObject MeteorVisual { get; set; }

	public void HideAll()
	{
		if ( DefaultBallVisual != null ) DefaultBallVisual.Enabled = false;
		if ( MeteorVisual != null ) MeteorVisual.Enabled = false;
	}
}

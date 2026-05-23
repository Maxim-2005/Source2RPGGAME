using Sandbox;
using MagicSystem;

public sealed class ProjectileVisuals : Component
{
	[Property] public GameObject DefaultBallVisual { get; set; }
	[Property] public GameObject MeteorVisual { get; set; }

	public void SetupVisuals( ProjectileType type, float scale )
	{
		if ( DefaultBallVisual != null )
			DefaultBallVisual.Enabled = (type == ProjectileType.Direct);

		if ( MeteorVisual != null )
			MeteorVisual.Enabled = (type == ProjectileType.Meteor);

		GameObject.WorldScale = new Vector3( scale );
	}

	public void HideAll()
	{
		if ( DefaultBallVisual != null ) DefaultBallVisual.Enabled = false;
		if ( MeteorVisual != null ) MeteorVisual.Enabled = false;
	}
}

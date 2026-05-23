using Sandbox;

public sealed class ExplosionBase : Component
{
	[Hide] public GameObject Launcher { get; private set; }

	public void SetupZone( GameObject launcher )
	{
		Launcher = launcher;
	}
}

using System.Collections.Generic;
using Sandbox;

public sealed class AoEExplosionDamage : Component
{
	public float Damage { get; set; } = 40f;
	public float Radius { get; set; } = 150f;
	public float ExplosionDebugLifetime { get; set; } = 0.5f;
	public bool ShowDebug { get; set; } = true;
	[Hide] public GameObject Launcher { get; set; }

	private bool _hasExploded = false;
	private TimeSince _timeSinceExplosion = 0f;

	public void Explode()
	{
		if ( _hasExploded ) return;
		_hasExploded = true;
		_timeSinceExplosion = 0f;

		GameObject launcher = Launcher;

		var hits = Scene.PhysicsWorld.Trace
			.Sphere( Radius, GameObject.WorldPosition, GameObject.WorldPosition )
			.RunAll();

		if ( hits == null ) return;

		HashSet<GameObject> hitObjects = new();

		foreach ( var hit in hits )
		{
			GameObject target = hit.Body?.GameObject;
			if ( target == null || hitObjects.Contains( target ) ) continue;
			if ( target.IsOwnedBy( launcher ) ) continue;
			if ( !target.Tags.Has( GameTags.Enemy ) ) continue;

			hitObjects.Add( target );

			var health = target.GetHealth();
			if ( health != null )
			{
				health.TakeDamage( Damage, launcher );
			}
		}
	}

	protected override void OnUpdate()
	{
		// ���� ����� ����������� ������ ����� � ���������� ������ ���� ���������
		if ( _timeSinceExplosion >= ExplosionDebugLifetime )
		{
			Destroy(); // ������� ��������� AoEExplosionDamage, ���� �� GameObject �������� ����!
			return;
		}

		// ���������� ��������� 3D ����� ������
		if ( ShowDebug )
		{
			using ( Gizmo.Scope() )
			{
				Gizmo.Transform = new Transform( GameObject.WorldPosition, Rotation.Identity );

				Gizmo.Draw.Color = Color.Orange.WithAlpha( 0.2f );
				Gizmo.Draw.SolidSphere( Vector3.Zero, Radius );

				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.LineSphere( new Sphere( Vector3.Zero, Radius ) );
			}
		}
	}
}

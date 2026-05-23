using System.Collections.Generic;
using Sandbox;

public sealed class AoEExplosionDamage : Component
{
	public float Damage { get; set; } = 40f;
	public float Radius { get; set; } = 150f;
	public float ExplosionDebugLifetime { get; set; } = 0.5f; // Сколько секунд виден взрыв (дебаг)
	public bool ShowDebug { get; set; } = true;

	private bool _hasExploded = false;
	private TimeSince _timeSinceExplosion = 0f;

	protected override void OnStart()
	{
		ExecuteExplosion();
		_timeSinceExplosion = 0f;
	}

	private void ExecuteExplosion()
	{
		if ( _hasExploded ) return;
		_hasExploded = true;

		var baseZone = GameObject.Components.Get<ExplosionBase>();
		GameObject launcher = baseZone != null ? baseZone.Launcher : null;

		var hits = Scene.PhysicsWorld.Trace
			.Sphere( Radius, GameObject.WorldPosition, GameObject.WorldPosition )
			.RunAll();

		if ( hits == null ) return;

		HashSet<GameObject> hitObjects = new();

		foreach ( var hit in hits )
		{
			GameObject target = hit.Body?.GameObject;
			if ( target == null || hitObjects.Contains( target ) ) continue;
			if ( target == launcher || target.Root == launcher?.Root ) continue;
			if ( !target.Tags.Has( "enemy" ) ) continue;

			hitObjects.Add( target );

			var health = target.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
			if ( health != null )
			{
				health.TakeDamage( Damage, launcher );
			}
		}
	}

	protected override void OnUpdate()
	{
		// Если время отображения взрыва вышло — уничтожаем только этот компонент
		if ( _timeSinceExplosion >= ExplosionDebugLifetime )
		{
			Destroy(); // Удаляет компонент AoEExplosionDamage, лужа на GameObject остается жить!
			return;
		}

		// Безопасная отрисовка 3D сферы взрыва
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

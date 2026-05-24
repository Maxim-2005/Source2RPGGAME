using Sandbox;

namespace MagicSystem
{
	public sealed class GasCloudDamage : AreaDamageOverTime
	{
		protected override string DebugPrefix => "GasCloud";
		protected override Color DebugColor => Color.Green.WithAlpha( 0.3f );

		protected override void ValidateDefaults()
		{
			if ( Radius <= 0f ) Radius = 150f;
			if ( TickInterval <= 0f ) TickInterval = 0.4f;
		}

		protected override bool IsTargetInZone( Vector3 zonePos, GameObject target, float radius )
		{
			float distance = Vector3.DistanceBetween( zonePos, target.WorldPosition );
			return distance <= (radius + 20f);
		}

		protected override void DrawDebugGizmos( Vector3 position, float radius )
		{
			Gizmo.Draw.LineSphere( position, radius );
		}
	}
}

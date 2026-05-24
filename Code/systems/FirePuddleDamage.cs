using Sandbox;

namespace MagicSystem
{
	public sealed class FirePuddleDamage : AreaDamageOverTime
	{
		[Property] public float MaxHeight { get; set; } = 40f;

		protected override string DebugPrefix => "Puddle";
		protected override Color DebugColor => Color.Red.WithAlpha( 0.4f );

		protected override void ValidateDefaults()
		{
			if ( Radius <= 0f ) Radius = 120f;
			if ( MaxHeight <= 0f ) MaxHeight = 40f;
			if ( TickInterval <= 0f ) TickInterval = 0.5f;
		}

		protected override Vector3 GetTraceEndOffset() => Vector3.Up * MaxHeight;

		protected override bool IsTargetInZone( Vector3 zonePos, GameObject target, float radius )
		{
			Vector3 offset = target.WorldPosition - zonePos;
			float horizontalDist = offset.WithZ( 0 ).Length;
			float verticalDist = offset.z;
			return horizontalDist <= radius && verticalDist >= -20f && verticalDist <= (MaxHeight + 20f);
		}

		protected override void DrawDebugGizmos( Vector3 position, float radius )
		{
			Gizmo.Draw.LineCircle( position, Vector3.Up, radius );
			Gizmo.Draw.LineCircle( position + Vector3.Up * MaxHeight, Vector3.Up, radius );
		}
	}
}

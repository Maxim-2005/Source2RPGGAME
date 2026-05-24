using Sandbox;

public static class AimHelper
{
	public struct AimResult
	{
		public Vector3 TargetPoint;
		public Vector3 Direction;
		public float Distance;
		public Vector3 HitNormal;
	}

	public static AimResult Calculate( Scene scene, Vector3 originPos, Vector3 cameraPos, Rotation cameraRot, float maxRange )
	{
		Vector3 traceStart = cameraPos;
		Vector3 traceEnd = cameraPos + cameraRot.Forward * 10000f;

		var cameraTrace = scene.PhysicsWorld.Trace
			.Ray( traceStart, traceEnd )
			.WithoutTags( "player", GameTags.Projectile, "trigger" )
			.Run();

		Vector3 rawTarget = cameraTrace.Hit ? cameraTrace.EndPosition : traceEnd;
		Vector3 toOrigin = rawTarget - originPos;
		float originDist = toOrigin.Length;
		Vector3 targetPoint = originDist <= maxRange
			? rawTarget
			: originPos + toOrigin.Normal * maxRange;

		if ( !cameraTrace.Hit )
		{
			var groundTrace = scene.PhysicsWorld.Trace
				.Ray( targetPoint, targetPoint + Vector3.Down * 5000f )
				.WithoutTags( "player", GameTags.Projectile, "trigger" )
				.Run();

			if ( groundTrace.Hit )
				targetPoint = groundTrace.EndPosition;
		}

		Vector3 direction = ( targetPoint - originPos ).Normal;
		float distance = ( targetPoint - originPos ).Length;
		Vector3 normal = cameraTrace.Hit ? cameraTrace.Normal : Vector3.Up;

		return new AimResult
		{
			TargetPoint = targetPoint,
			Direction = direction,
			Distance = distance,
			HitNormal = normal
		};
	}
}

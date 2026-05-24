using Sandbox;
using MagicSystem;
using System;
using System.Linq;

public sealed class SpellTargetIndicator : Component
{
	[Property, Group( "Indicator Setup" )] public GameObject IndicatorPrefab { get; set; }
	[Property, Group( "Indicator Setup" )] public float CircleModelRadius { get; set; } = 100f;

	private GameObject _indicatorInstance;
	private IAreaRadiusProvider _radiusProvider;
	private WeaponItem _weaponItem;
	private CameraComponent _camera;
	// ������� ������ ��� �������������� ������ � ������� ����������� ��� ������ �����
	private int _initialFramesDelay = 0;

	protected override void OnStart()
	{
		_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		if ( _camera == null )
		{
			_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
			if ( _camera == null )
			{
				HideIndicator();
				return;
			}
		}

		// ���������� ������ 5 ������ ����, ���� ����� ��������������� � ����� ������
		if ( _initialFramesDelay < 5 )
		{
			_initialFramesDelay++;
			HideIndicator();
			return;
		}

		if ( _weaponItem == null )
		{
			_weaponItem = Components.Get<WeaponItem>();
		}

		if ( _weaponItem == null || !_weaponItem.IsHeld )
		{
			HideIndicator();
			return;
		}

		_radiusProvider = Components.Get<IAreaRadiusProvider>();
		if ( _radiusProvider == null ) _radiusProvider = Components.GetInDescendants<IAreaRadiusProvider>();
		if ( _radiusProvider == null ) _radiusProvider = Components.GetInAncestors<IAreaRadiusProvider>();

		if ( _radiusProvider == null || _radiusProvider.IsAttacking )
		{
			HideIndicator();
			return;
		}

		// ����������� �� ������ �� ��������� (��� �������� 3rd person — ������ ����� ����� ��� MaxRange)
		Vector3 traceStart = _camera.WorldPosition;
		Vector3 traceDirection = _camera.WorldRotation.Forward;
		Vector3 traceEnd = traceStart + traceDirection * 10000f;

		var trace = Scene.PhysicsWorld.Trace
			.Ray( traceStart, traceEnd )
			.WithoutTag( "player" )
			.WithoutTag( "projectile" )
			.WithoutTag( "trigger" )
			.Run();

		float maxRange = _radiusProvider.MaxRange;
		Vector3 rawTarget = trace.Hit ? trace.EndPosition : traceEnd;
		Vector3 toWeapon = rawTarget - GameObject.WorldPosition;
		float weaponDist = toWeapon.Length;

		Vector3 finalTarget = weaponDist <= maxRange
			? rawTarget
			: GameObject.WorldPosition + toWeapon.Normal * maxRange;

		if ( !trace.Hit )
		{
			var groundTrace = Scene.PhysicsWorld.Trace
				.Ray( finalTarget, finalTarget + Vector3.Down * 5000f )
				.WithoutTag( "player" )
				.WithoutTag( "projectile" )
				.WithoutTag( "trigger" )
				.Run();

			if ( groundTrace.Hit )
				finalTarget = groundTrace.EndPosition;
		}

		PhysicsTraceResult indicatorTrace = new PhysicsTraceResult
		{
			Hit = true,
			EndPosition = finalTarget,
			Normal = trace.Hit ? trace.Normal : Vector3.Up
		};
		UpdateIndicator( indicatorTrace );
	}

	private void UpdateIndicator( PhysicsTraceResult trace )
	{
		if ( IndicatorPrefab == null ) return;

		if ( _indicatorInstance == null )
		{
			_indicatorInstance = IndicatorPrefab.Clone();
		}

		if ( _indicatorInstance == null ) return;

		_indicatorInstance.WorldPosition = trace.EndPosition;
		_indicatorInstance.WorldRotation = Rotation.FromToRotation( Vector3.Up, trace.Normal );

		var directVisual = _indicatorInstance.Children.Find( c => c.Name == "DirectPoint" );
		var areaVisual = _indicatorInstance.Children.Find( c => c.Name == "AreaCircle" );

		if ( _radiusProvider.MagicType == ProjectileType.Direct )
		{
			if ( directVisual != null ) directVisual.Enabled = true;
			if ( areaVisual != null ) areaVisual.Enabled = false;
		}
		else if ( _radiusProvider.MagicType == ProjectileType.Meteor )
		{
			if ( directVisual != null ) directVisual.Enabled = false;
			if ( areaVisual != null ) areaVisual.Enabled = true;

			if ( areaVisual != null )
			{
				float maxRadius = _radiusProvider.GetMaxAreaRadius();
				float targetScale = maxRadius / CircleModelRadius;
				areaVisual.LocalScale = new Vector3( targetScale, targetScale, targetScale );
			}
		}
	}

	private void HideIndicator()
	{
		if ( _indicatorInstance != null )
		{
			_indicatorInstance.Destroy();
			_indicatorInstance = null;
		}
	}

	protected override void OnDestroy()
	{
		HideIndicator();
	}
}

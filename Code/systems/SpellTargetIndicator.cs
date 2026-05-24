using Sandbox;
using MagicSystem;
using System;
using System.Linq;

public sealed class SpellTargetIndicator : Component
{
	[Property, Group( "Indicator Setup" )] public GameObject IndicatorPrefab { get; set; }
	[Property, Group( "Indicator Setup" )] public float CircleModelRadius { get; set; } = 100f;
	[Property, Group( "Indicator Setup" )] public string DirectPointName { get; set; } = "DirectPoint";
	[Property, Group( "Indicator Setup" )] public string AreaCircleName { get; set; } = "AreaCircle";

	private GameObject _indicatorInstance;
	private IAreaRadiusProvider _radiusProvider;
	private WeaponItem _weaponItem;
	private CameraComponent _camera;
	private bool _isInitialized = false;

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

		if ( !_isInitialized )
		{
			_weaponItem = Components.Get<WeaponItem>();
			_radiusProvider = Components.Get<IAreaRadiusProvider>()
			                  ?? Components.GetInDescendants<IAreaRadiusProvider>()
			                  ?? Components.GetInAncestors<IAreaRadiusProvider>();
			if ( _weaponItem != null && _radiusProvider != null )
				_isInitialized = true;
			else
			{
				HideIndicator();
				return;
			}
		}

		if ( !_weaponItem.IsHeld )
		{
			HideIndicator();
			return;
		}

		if ( _radiusProvider.IsAttacking )
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
			.WithoutTag( GameTags.Projectile )
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
				.WithoutTag( GameTags.Projectile )
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

		var directVisual = _indicatorInstance.Children.Find( c => c.Name == DirectPointName );
		var areaVisual = _indicatorInstance.Children.Find( c => c.Name == AreaCircleName );

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

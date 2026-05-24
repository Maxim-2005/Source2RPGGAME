using Sandbox;
using System;
using System.Linq;

public sealed class SpellTargetIndicator : Component
{
	[Property, Group( "Indicator Setup" )] public GameObject IndicatorPrefab { get; set; }
	[Property, Group( "Indicator Setup" )] public float CircleModelRadius { get; set; } = 100f;
	[Property, Group( "Indicator Setup" )] public string DirectPointName { get; set; } = "DirectPoint";
	[Property, Group( "Indicator Setup" )] public string AreaCircleName { get; set; } = "AreaCircle";

	[Hide] private GameObject _indicatorInstance;
	[Hide] private IAreaRadiusProvider _radiusProvider;
	[Hide] private WeaponItem _weaponItem;
	[Hide] private CameraComponent _camera;
	[Hide] private bool _isInitialized = false;

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

		if ( !_radiusProvider.Behavior.IsAreaTarget )
		{
			HideIndicator();
			return;
		}

		float maxRange = _radiusProvider.MaxRange;
		var aim = AimHelper.Calculate( Scene, GameObject.WorldPosition, _camera.WorldPosition, _camera.WorldRotation, maxRange );

		PhysicsTraceResult indicatorTrace = new PhysicsTraceResult
		{
			Hit = true,
			EndPosition = aim.TargetPoint,
			Normal = aim.HitNormal
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

		if ( directVisual != null ) directVisual.Enabled = false;
		if ( areaVisual != null ) areaVisual.Enabled = true;

		if ( areaVisual != null )
		{
			float maxRadius = _radiusProvider.GetMaxAreaRadius();
			float targetScale = maxRadius / CircleModelRadius;
			areaVisual.LocalScale = new Vector3( targetScale, targetScale, targetScale );
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

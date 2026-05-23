using Sandbox;
using MagicSystem;
using System;
using System.Linq;

public sealed class SpellTargetIndicator : Component
{
	[Property, Group( "Indicator Setup" )] public GameObject IndicatorPrefab { get; set; }
	[Property, Group( "Indicator Setup" )] public float MaxTraceDistance { get; set; } = 2500f;

	private GameObject _indicatorInstance;
	private AttackProjectile _activeWeaponModule;
	private CameraComponent _camera;
	private GameObject _launchPoint;

	// ������� ������ ��� �������������� ������ � ������� ����������� ��� ������ �����
	private int _initialFramesDelay = 0;

	protected override void OnStart()
	{
		_camera = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		// ���������� ������ 5 ������ ����, ���� ����� ��������������� � ����� ������
		if ( _initialFramesDelay < 5 )
		{
			_initialFramesDelay++;
			HideIndicator();
			return;
		}

		// ���� ������ ����� �� ������
		_activeWeaponModule = Components.Get<AttackProjectile>();
		if ( _activeWeaponModule == null ) _activeWeaponModule = Components.GetInDescendants<AttackProjectile>();
		if ( _activeWeaponModule == null ) _activeWeaponModule = Components.GetInAncestors<AttackProjectile>();

		if ( _activeWeaponModule == null || _activeWeaponModule.IsAttacking )
		{
			HideIndicator();
			return;
		}

		// ���� �������� LaunchPoint
		if ( _launchPoint == null )
		{
			_launchPoint = GameObject.Children.Find( c => c.Name == "LaunchPoint" );
		}

		// ����� ������ �� ��������� � ��� �����
		Vector3 traceStart = GameObject.WorldPosition;
		Vector3 traceDirection = GameObject.WorldRotation.Forward;

		// ���� ����� LaunchPoint � ����� ��� ���������� � ���������� ��������� ������
		if ( _launchPoint != null )
		{
			traceDirection = _launchPoint.WorldRotation.Forward;
			traceStart = _launchPoint.WorldPosition + traceDirection * 15f;
		}
		else
		{
			traceStart = GameObject.WorldPosition + traceDirection * 15f;
		}

		Vector3 traceEnd = traceStart + traceDirection * MaxTraceDistance;

		// ����� ��� ���� "world", ����� �������������� �������� ����� ����������� ��� ������
		var trace = Scene.PhysicsWorld.Trace
			.Ray( traceStart, traceEnd )
			.WithoutTag( "player" )
			.WithoutTag( "projectile" )
			.WithoutTag( "trigger" )
			.Run();

		if ( trace.Hit )
		{
			UpdateIndicator( trace );
		}
		else
		{
			HideIndicator();
		}
	}

	private void UpdateIndicator( PhysicsTraceResult trace )
	{
		if ( IndicatorPrefab == null ) return;

		if ( _indicatorInstance == null )
		{
			_indicatorInstance = IndicatorPrefab.Clone();
		}

		if ( _indicatorInstance == null ) return;

		// ������������� ������� � ����� ���������
		_indicatorInstance.WorldPosition = trace.EndPosition;

		// �������������� ����: ������ LookAt ���������� FromToRotation.
		// ��� ����� ��������� ������ Up (��������� ����� �� �����) � ���������� ��� �� ������� �����.
		// ���� ������ ������ ����� ������ ������ �� ���� � �������, �� ������������� �������.
		_indicatorInstance.WorldRotation = Rotation.FromToRotation( Vector3.Up, trace.Normal );

		var directVisual = _indicatorInstance.Children.Find( c => c.Name == "DirectPoint" );
		var areaVisual = _indicatorInstance.Children.Find( c => c.Name == "AreaCircle" );

		if ( _activeWeaponModule.MagicType == ProjectileType.Direct )
		{
			if ( directVisual != null ) directVisual.Enabled = true;
			if ( areaVisual != null ) areaVisual.Enabled = false;
		}
		else if ( _activeWeaponModule.MagicType == ProjectileType.Meteor )
		{
			if ( directVisual != null ) directVisual.Enabled = false;
			if ( areaVisual != null ) areaVisual.Enabled = true;

			if ( areaVisual != null )
			{
				float maxRadius = 0f;

				if ( _activeWeaponModule.Explosion.Enabled )
				{
					float checkRad = (float)_activeWeaponModule.Explosion.Radius;
					if ( checkRad > maxRadius ) maxRadius = checkRad;
				}

				if ( _activeWeaponModule.Puddle.Enabled )
				{
					float checkRad = (float)_activeWeaponModule.Puddle.Radius;
					if ( checkRad > maxRadius ) maxRadius = checkRad;
				}

				if ( _activeWeaponModule.Gas.Enabled )
				{
					float checkRad = (float)_activeWeaponModule.Gas.Radius;
					if ( checkRad > maxRadius ) maxRadius = checkRad;
				}

				// ���������: ���� ������� ��� �� ������ ������������ �� ������� ������,
				// ������ ������� ���������� ������ �� ���������, ����� �������� ������ � 0
				if ( maxRadius <= 0f ) maxRadius = 150f;

				float targetScale = maxRadius / 100f;

				// ��� ��� ���� ������ ����� �������� ������, ��� Z �������� �� ��� �������.
				// ������ � � 1f, ����� �������� �� ����������� � ������, � X � Y ����� �������� �� ������.
				areaVisual.WorldScale = new Vector3( targetScale, targetScale, 1f );
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

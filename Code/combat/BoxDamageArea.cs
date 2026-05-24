using System;
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

// ����������� �� ������ ������������ ������ �����, � �� �� Component
public sealed class BoxDamageArea : BaseAttackModule
{
	[Property, Group( "Damage Settings" )] public float Damage { get; set; } = 25f;

	[Property, Group( "Capsule Size" )] public float CapsuleRadius { get; set; } = 20f;
	[Property, Group( "Capsule Size" )] public float CapsuleLength { get; set; } = 60f;

	[Property, Group( "Setup" )] public GameObject AttackOrigin { get; set; }
	[Property, Group( "Setup" )] public bool ShowDebug { get; set; } = true;

	// �������� �����, ������� ������ ���� � WeaponItem, ������ ����� � ����� ������ ����
	[Property, Group( "Attack Timings" )] public float PreAttackDelay { get; set; } = 0.15f;
	[Property, Group( "Attack Timings" )] public float AttackDuration { get; set; } = 0.2f;
	[Property, Group( "Attack Timings" )] public float AttackCooldown { get; set; } = 0.6f;

	[Hide] public bool IsAttackActive { get; set; } = false;

	private GameObject AttackOriginPoint => AttackOrigin != null ? AttackOrigin : GameObject;

	[Hide] private readonly HashSet<GameObject> _hitTargetsThisAttack = new();
	[Hide] private TimeSince _timeSinceLastAttack = 999f; // ������ ��������

	protected override void OnStart()
	{
		_timeSinceLastAttack = 999f;
	}

	protected override void OnUpdate()
	{
		if ( !ShowDebug ) return;

		GameObject originPoint = AttackOriginPoint;
		if ( originPoint == null ) return;

		using ( Gizmo.Scope() )
		{
			Gizmo.Transform = new Transform( originPoint.WorldPosition, originPoint.WorldRotation );

			// �����: ������� ���� ����, �������������� ������� ���� ������ ������
			Gizmo.Draw.Color = IsAttackActive ? Color.Red : Color.Green.WithAlpha( 0.4f );

			Vector3 capsuleStart = Vector3.Zero;
			Vector3 capsuleEnd = Vector3.Forward * CapsuleLength;

			Gizmo.Draw.LineCapsule( new Capsule( capsuleStart, capsuleEnd, CapsuleRadius ) );
		}
	}

	/// <summary>
	/// ���������� �� BaseAttackModule: ���������� �� WeaponItem, ����� ����� ���
	/// </summary>
	public override bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		// ���� ��� ����� ����� ��� �� ������ ������� � ���������� ����
		if ( IsAttacking || _timeSinceLastAttack < AttackCooldown ) return false;
		if ( attacker == null ) return false;

		// ��������� ����������� ������� �����
		_ = ProcessSwing( attacker, playerModel );

		return true;
	}

	private async Task ProcessSwing( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		_timeSinceLastAttack = 0;
		await RunAttackAsync( PreAttackDelay, playerModel, "b_attack", async () =>
		{
			ResetTargets();
			IsAttackActive = true;

			TimeSince timeSinceAttackStart = 0;
			while ( timeSinceAttackStart < AttackDuration )
			{
				if ( !IsValid || !GameObject.IsValid() || GameObject.Parent == null ) break;

				ExecuteDamageTick( attacker );
				await Task.Frame();
			}

			IsAttackActive = false;
		} );
	}


	/// <summary>
	/// �������������� ��������� ����� (��������, ���� ��������� ������)
	/// </summary>
	public override void StopAttack()
	{
		IsAttackActive = false;
	}

	public void ResetTargets()
	{
		_hitTargetsThisAttack.Clear();
	}

	public void ExecuteDamageTick( GameObject attacker )
	{
		GameObject originPoint = AttackOriginPoint;
		if ( originPoint == null ) return;

		Vector3 startSweep = originPoint.WorldPosition;
		Vector3 endSweep = startSweep + (originPoint.WorldRotation.Forward * CapsuleLength);

		// ���� CS0103: ���������� ������ �������� ����� ��� ��������� �����
		var allHits = Scene.PhysicsWorld.Trace
			.Sphere( CapsuleRadius, startSweep, endSweep )
			.RunAll();

		if ( allHits == null ) return;

		foreach ( var hitResult in allHits )
		{
			GameObject target = hitResult.Body?.GameObject;
			if ( target == null ) continue;

			if ( target == attacker || target == GameObject || target.Tags.Has( GameTags.Player ) ) continue;
			if ( !target.Tags.Has( GameTags.Enemy ) ) continue;

			if ( !_hitTargetsThisAttack.Contains( target ) )
			{
				_hitTargetsThisAttack.Add( target );
				DamageService.ApplyDamage( target, Damage, attacker );
			}
		}
	}
}

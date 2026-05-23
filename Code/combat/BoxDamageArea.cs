using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

// Наследуемся от нашего абстрактного модуля атаки, а не от Component
public sealed class BoxDamageArea : BaseAttackModule
{
	[Property, Group( "Damage Settings" )] public float Damage { get; set; } = 25f;

	[Property, Group( "Capsule Size" )] public float CapsuleRadius { get; set; } = 20f;
	[Property, Group( "Capsule Size" )] public float CapsuleLength { get; set; } = 60f;

	[Property, Group( "Setup" )] public GameObject AttackOrigin { get; set; }
	[Property, Group( "Setup" )] public bool ShowDebug { get; set; } = true;

	// Тайминги удара, которые раньше были в WeaponItem, теперь живут в самом модуле меча
	[Property, Group( "Attack Timings" )] public float PreAttackDelay { get; set; } = 0.15f;
	[Property, Group( "Attack Timings" )] public float AttackDuration { get; set; } = 0.2f;
	[Property, Group( "Attack Timings" )] public float AttackCooldown { get; set; } = 0.6f;

	[HideInEditor] public bool IsAttackActive { get; set; } = false;

	private readonly HashSet<GameObject> _hitTargetsThisAttack = new();
	private TimeSince _timeSinceLastAttack = 999f; // Таймер кулдауна

	protected override void OnStart()
	{
		_timeSinceLastAttack = 999f;
	}

	protected override void OnUpdate()
	{
		if ( !ShowDebug ) return;

		GameObject originPoint = AttackOrigin != null ? AttackOrigin : GameObject;
		if ( originPoint == null ) return;

		using ( Gizmo.Scope() )
		{
			Gizmo.Transform = new Transform( originPoint.WorldPosition, originPoint.WorldRotation );

			// Дебаг: красный если бьем, полупрозрачный зеленый если просто держим
			Gizmo.Draw.Color = IsAttackActive ? Color.Red : Color.Green.WithAlpha( 0.4f );

			Vector3 capsuleStart = Vector3.Zero;
			Vector3 capsuleEnd = Vector3.Forward * CapsuleLength;

			Gizmo.Draw.LineCapsule( new Capsule( capsuleStart, capsuleEnd, CapsuleRadius ) );
		}
	}

	/// <summary>
	/// РЕАЛИЗАЦИЯ ИЗ BaseAttackModule: вызывается из WeaponItem, когда зажат ЛКМ
	/// </summary>
	public override bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		// Если уже машем мечом или не прошел кулдаун — игнорируем клик
		if ( IsAttacking || _timeSinceLastAttack < AttackCooldown ) return false;
		if ( attacker == null ) return false;

		// Запускаем асинхронный процесс удара
		_ = ProcessSwing( attacker, playerModel );

		return true;
	}

	private async Task ProcessSwing( GameObject attacker, SkinnedModelRenderer playerModel )
	{
		IsAttacking = true;
		_timeSinceLastAttack = 0; // Сбрасываем кулдаун

		if ( playerModel != null )
		{
			playerModel.Set( "b_attack", true );
		}

		// 1. Фаза замаха (PreAttackDelay)
		await Task.DelaySeconds( PreAttackDelay );

		// Защита на случай, если во время замаха оружие выбросили или удалили
		if ( !IsValid || !GameObject.IsValid() || GameObject.Parent == null )
		{
			IsAttacking = false;
			return;
		}

		// 2. Активная фаза удара (наносим урон)
		ResetTargets();
		IsAttackActive = true;

		TimeSince timeSinceAttackStart = 0;
		while ( timeSinceAttackStart < AttackDuration )
		{
			if ( !IsValid || !GameObject.IsValid() || GameObject.Parent == null ) break;

			ExecuteDamageTick( attacker );
			await Task.Frame();
		}

		// 3. Конец атаки
		IsAttackActive = false;
		IsAttacking = false;
	}

	/// <summary>
	/// Принудительная остановка атаки (например, если выбросили оружие)
	/// </summary>
	public override void StopAttack()
	{
		if ( !IsAttacking )
		{
			IsAttackActive = false;
		}
	}

	public void ResetTargets()
	{
		_hitTargetsThisAttack.Clear();
	}

	public void ExecuteDamageTick( GameObject attacker )
	{
		GameObject originPoint = AttackOrigin != null ? AttackOrigin : GameObject;
		if ( originPoint == null ) return;

		Vector3 startSweep = originPoint.WorldPosition;
		Vector3 endSweep = startSweep + (originPoint.WorldRotation.Forward * CapsuleLength);

		// ФИКС CS0103: Используем точный контекст сцены для трейсинга сферы
		var allHits = Scene.PhysicsWorld.Trace
			.Sphere( CapsuleRadius, startSweep, endSweep )
			.RunAll();

		if ( allHits == null ) return;

		foreach ( var hitResult in allHits )
		{
			GameObject target = hitResult.Body?.GameObject;
			if ( target == null ) continue;

			if ( target == attacker || target == GameObject || target.Name.Contains( "Player" ) ) continue;
			if ( !target.Tags.Has( "enemy" ) ) continue;

			if ( !_hitTargetsThisAttack.Contains( target ) )
			{
				var health = target.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors )
							 ?? target.Components.Get<HealthComponent>( FindMode.EverythingInDescendants );

				if ( health != null )
				{
					_hitTargetsThisAttack.Add( target );
					health.TakeDamage( Damage, attacker );
				}
			}
		}
	}
}

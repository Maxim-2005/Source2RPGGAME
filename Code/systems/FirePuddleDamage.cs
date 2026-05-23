using System;
using System.Collections.Generic;
using Sandbox;

namespace MagicSystem
{
	public sealed class FirePuddleDamage : Component
	{
		[Property] public float DamagePerTick { get; set; } = 5f;
		[Property] public float TickInterval { get; set; } = 0.5f;
		[Property] public float Radius { get; set; } = 120f;
		[Property] public float MaxHeight { get; set; } = 40f; // Регулируется из инспектора оружия/префаба
		[Property] public float PuddleLifetime { get; set; } = 4f;
		[Property] public bool ShowDebug { get; set; } = true; // <--- ТЕПЕРЬ ВКЛЮЧЕНО ПО УМОЛЧАНИЮ

		private RealTimeSince _timeSinceSpawn;
		private Dictionary<GameObject, float> _burnTimers = new();

		protected override void OnStart()
		{
			_timeSinceSpawn = 0f;

			// ЗАЩИТА: Если из инспектора прилетели нули, возвращаем рабочие дефолты
			if ( Radius <= 0f ) Radius = 120f;
			if ( MaxHeight <= 0f ) MaxHeight = 40f;
			if ( TickInterval <= 0f ) TickInterval = 0.5f;

			if ( ShowDebug )
			{
				Log.Info( $"[Puddle] Скрипт FirePuddleDamage запущен! Радиус: {Radius}, Высота: {MaxHeight}" );
			}
		}

		protected override void OnUpdate()
		{
			// Проверяем время жизни лужи
			if ( _timeSinceSpawn >= PuddleLifetime )
			{
				GameObject.Destroy();
				return;
			}

			// БЕЗОПАСНАЯ ОТРИСОВКА ДЕБАГ-ЗОНЫ
			if ( ShowDebug )
			{
				using ( Gizmo.Scope() ) // <--- Изолируем рисование, чтобы не ломать GameObject
				{
					Gizmo.Draw.Color = Color.Red.WithAlpha( 0.4f );
					// Рисуем нижний круг (основание лужи на земле)
					Gizmo.Draw.LineCircle( GameObject.WorldPosition, Vector3.Up, Radius );
					// Рисуем верхний круг (границу высоты)
					Gizmo.Draw.LineCircle( GameObject.WorldPosition + Vector3.Up * MaxHeight, Vector3.Up, Radius );
				}
			}

			var baseZone = GameObject.Components.Get<ExplosionBase>();
			GameObject launcher = baseZone != null ? baseZone.Launcher : null;

			Vector3 puddlePos = GameObject.WorldPosition;

			// Вертикальный свип (протаскивание сферы от земли до MaxHeight)
			Vector3 traceStart = puddlePos;
			Vector3 traceEnd = puddlePos + Vector3.Up * MaxHeight;

			var hits = Scene.PhysicsWorld.Trace
				.Sphere( Radius, traceStart, traceEnd )
				.WithTag( "enemy" )
				.RunAll();

			HashSet<GameObject> currentValidTargets = new();
			float deltaTime = Time.Delta;

			// Обновляем таймеры для уже горящих врагов
			var activeKeys = new List<GameObject>( _burnTimers.Keys );
			foreach ( var key in activeKeys )
			{
				if ( key.IsValid() )
				{
					_burnTimers[key] += deltaTime;
				}
			}

			if ( hits != null )
			{
				foreach ( var hit in hits )
				{
					GameObject target = hit.Body?.GameObject;
					if ( target == null || !target.IsValid() ) continue;

					// Не дамажим создателя магии
					if ( launcher != null && (target == launcher || target.Root == launcher.Root) ) continue;

					// Считаем вектор смещения от центра лужи до центра врага
					Vector3 offset = target.WorldPosition - puddlePos;

					float horizontalDist = offset.WithZ( 0 ).Length; // Расстояние по горизонтали
					float verticalDist = offset.z;                  // Высота центра врага относительно лужи

					// Проверка цилиндра:
					if ( horizontalDist > Radius || verticalDist < -20f || verticalDist > (MaxHeight + 20f) ) continue;

					currentValidTargets.Add( target );

					// Если враг только что наступил/влетел в лужу
					if ( !_burnTimers.ContainsKey( target ) )
					{
						_burnTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
					// Если подошло время следующего тика урона
					else if ( _burnTimers[target] >= TickInterval )
					{
						_burnTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
				}
			}

			// Очищаем таймеры для тех объектов, которые вышли из лужи или умерли
			foreach ( var key in activeKeys )
			{
				if ( !currentValidTargets.Contains( key ) || !key.IsValid() )
				{
					_burnTimers.Remove( key );
				}
			}
		}

		private void ApplyDamage( GameObject target, GameObject launcher )
		{
			if ( ShowDebug )
			{
				Log.Info( $"[Puddle] Урон по: {target.Name} от {launcher?.Name}" );
			}

			var health = target.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors )
						 ?? target.Components.Get<HealthComponent>( FindMode.EverythingInDescendants );

			if ( health != null )
			{
				health.TakeDamage( DamagePerTick, launcher );
			}
		}
	}
}

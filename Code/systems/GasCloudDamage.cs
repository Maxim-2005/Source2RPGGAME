using System;
using System.Collections.Generic;
using Sandbox;

namespace MagicSystem
{
	public sealed class GasCloudDamage : Component
	{
		[Property] public float DamagePerTick { get; set; } = 8f;
		[Property] public float TickInterval { get; set; } = 0.4f;
		[Property] public float Radius { get; set; } = 150f; // Радиус сферы газового облака
		[Property] public float CloudLifetime { get; set; } = 6f;
		[Property] public bool ShowDebug { get; set; } = true;

		private RealTimeSince _timeSinceSpawn;
		private Dictionary<GameObject, float> _gasTimers = new();

		protected override void OnStart()
		{
			_timeSinceSpawn = 0f;

			// Защита от сброса значений в инспекторе
			if ( Radius <= 0f ) Radius = 150f;
			if ( TickInterval <= 0f ) TickInterval = 0.4f;

			if ( ShowDebug )
			{
				Log.Info( $"[GasCloud] Облако запущено! Радиус сферы: {Radius}, Время жизни: {CloudLifetime}" );
			}
		}

		protected override void OnUpdate()
		{
			// Проверяем время жизни газового облака
			if ( _timeSinceSpawn >= CloudLifetime )
			{
				GameObject.Destroy();
				return;
			}

			// Безопасная отрисовка 3D-сферы газа через Gizmo Scope
			if ( ShowDebug )
			{
				using ( Gizmo.Scope() )
				{
					Gizmo.Draw.Color = Color.Green.WithAlpha( 0.3f ); // Зеленый ядовитый газ
					Gizmo.Draw.LineSphere( GameObject.WorldPosition, Radius );
				}
			}

			// Вытаскиваем кастера заклинания через ExplosionBase
			var baseZone = GameObject.Components.Get<ExplosionBase>();
			GameObject launcher = baseZone != null ? baseZone.Launcher : null;

			Vector3 cloudPos = GameObject.WorldPosition;

			// Собираем всех врагов внутри сферы газа с помощью статической проверки (Overlap)
			var hits = Scene.PhysicsWorld.Trace
				.Sphere( Radius, cloudPos, cloudPos )
				.WithTag( "enemy" )
				.RunAll();

			HashSet<GameObject> currentValidTargets = new();
			float deltaTime = Time.Delta;

			// Обновляем таймеры тиков для тех, кто уже находится в газе
			var activeKeys = new List<GameObject>( _gasTimers.Keys );
			foreach ( var key in activeKeys )
			{
				if ( key.IsValid() )
				{
					_gasTimers[key] += deltaTime;
				}
			}

			if ( hits != null )
			{
				foreach ( var hit in hits )
				{
					GameObject target = hit.Body?.GameObject;
					if ( target == null || !target.IsValid() ) continue;

					// Кастер не должен получать урон от своего газа
					if ( launcher != null && (target == launcher || target.Root == launcher.Root) ) continue;

					// Честная проверка расстояния между центром облака и центром врага (для сферической зоны)
					float distance = Vector3.DistanceBetween( cloudPos, target.WorldPosition );

					// Если враг вышел за пределы сферы (с учетом небольшого запаса на размер коллайдера +20 единиц)
					if ( distance > (Radius + 20f) ) continue;

					currentValidTargets.Add( target );

					// Если враг только что зашел/влетел в облако
					if ( !_gasTimers.ContainsKey( target ) )
					{
						_gasTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
					// Если подошло время следующего тика отравления
					else if ( _gasTimers[target] >= TickInterval )
					{
						_gasTimers[target] = 0f;
						ApplyDamage( target, launcher );
					}
				}
			}

			// Убираем таймеры для тех, кто покинул облако или погиб
			foreach ( var key in activeKeys )
			{
				if ( !currentValidTargets.Contains( key ) || !key.IsValid() )
				{
					_gasTimers.Remove( key );
				}
			}
		}

		private void ApplyDamage( GameObject target, GameObject launcher )
		{
			if ( ShowDebug )
			{
				Log.Info( $"[GasCloud] Урон ядом по: {target.Name} от {launcher?.Name}" );
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

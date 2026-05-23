using System;
using System.Collections.Generic;
using Sandbox;

public sealed class DamageZone : Component, Component.ITriggerListener
{
	[Property, Group( "Damage" )] public float BaseDamage { get; set; } = 30f;
	[Property, Group( "Damage" )] public bool IsAoEExplosion { get; set; } = true;
	[Property, Group( "Damage" )] public bool IsDotZone { get; set; } = false;
	[Property, Group( "Damage" )] public float DotInterval { get; set; } = 1f;

	[Property, Group( "Lifetime" )] public float Lifetime { get; set; } = 3f;

	private GameObject _launcher;
	private Dictionary<GameObject, TimeSince> _dotTargets = new();
	private List<GameObject> _insideZone = new();

	public void SetupZone( GameObject launcher )
	{
		_launcher = launcher;
	}

	protected override void OnStart()
	{
		// Если это мгновенный взрыв, наносим урон всем, кто уже внутри триггера на первом кадре
		if ( IsAoEExplosion )
		{
			ExecuteExplosion();
		}

		// Запускаем уничтожение объекта зоны по истечении Lifetime
		if ( Lifetime > 0 && GameObject != null )
		{
			_ = DeleteAfterDelay( Lifetime );
		}
	}

	private async System.Threading.Tasks.Task DeleteAfterDelay( float delay )
	{
		await GameTask.DelaySeconds( delay );
		if ( GameObject.IsValid() )
		{
			GameObject.Destroy();
		}
	}

	protected override void OnUpdate()
	{
		if ( !IsDotZone ) return;

		// Пробегаемся по врагам в луже и наносим урон по таймеру DoTInterval
		for ( int i = _insideZone.Count - 1; i >= 0; i-- )
		{
			var target = _insideZone[i];
			if ( target == null || !target.IsValid() )
			{
				_insideZone.RemoveAt( i );
				continue;
			}

			if ( !_dotTargets.ContainsKey( target ) )
			{
				_dotTargets[target] = 0f;
				ApplyDamage( target );
			}
			else if ( _dotTargets[target] >= DotInterval )
			{
				_dotTargets[target] = 0f;
				ApplyDamage( target );
			}
		}
	}

	private void ExecuteExplosion()
	{
		foreach ( var target in _insideZone )
		{
			ApplyDamage( target );
		}

		// Если это просто взрыв (не лужа), объект можно удалять сразу после нанесения AoE урона
		if ( !IsDotZone )
		{
			GameObject.Destroy();
		}
	}

	private void ApplyDamage( GameObject target )
	{
		// Не бьем создателя взрыва
		if ( target == null || target == _launcher || target.Root == _launcher?.Root ) return;

		var health = target.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors )
					 ?? target.Components.Get<HealthComponent>( FindMode.EverythingInDescendants );

		if ( health != null && target.Tags.Has( "enemy" ) )
		{
			health.TakeDamage( BaseDamage, _launcher );
		}
	}

	// Встроенные методы S&box для регистрации объектов в Collider-триггере
	public void OnTriggerEnter( Collider other )
	{
		if ( other?.GameObject == null ) return;

		if ( !_insideZone.Contains( other.GameObject ) )
		{
			_insideZone.Add( other.GameObject );
		}
	}

	public void OnTriggerExit( Collider other )
	{
		if ( other?.GameObject == null ) return;

		if ( _insideZone.Contains( other.GameObject ) )
		{
			_insideZone.Remove( other.GameObject );
			if ( _dotTargets.ContainsKey( other.GameObject ) )
			{
				_dotTargets.Remove( other.GameObject );
			}
		}
	}
}

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
		// ���� ��� ���������� �����, ������� ���� ����, ��� ��� ������ �������� �� ������ �����
		if ( IsAoEExplosion )
		{
			ExecuteExplosion();
		}

		// ��������� ����������� ������� ���� �� ��������� Lifetime
		if ( Lifetime > 0 && GameObject != null )
		{
			_ = DeleteAfterDelay( Lifetime );
		}
	}

	private async System.Threading.Tasks.Task DeleteAfterDelay( float delay )
	{
		try
		{
			await GameTask.DelaySeconds( delay );
			if ( GameObject.IsValid() )
			{
				GameObject.Destroy();
			}
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
	}

	protected override void OnUpdate()
	{
		if ( !IsDotZone ) return;

		// ����������� �� ������ � ���� � ������� ���� �� ������� DoTInterval
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

		// ���� ��� ������ ����� (�� ����), ������ ����� ������� ����� ����� ��������� AoE �����
		if ( !IsDotZone )
		{
			GameObject.Destroy();
		}
	}

	private void ApplyDamage( GameObject target )
	{
		// �� ���� ��������� ������
		if ( target == null || target.IsOwnedBy( _launcher ) ) return;

		var health = target.GetHealth();

		if ( health != null && target.Tags.Has( GameTags.Enemy ) )
		{
			health.TakeDamage( BaseDamage, _launcher );
		}
	}

	// ���������� ������ S&box ��� ����������� �������� � Collider-��������
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

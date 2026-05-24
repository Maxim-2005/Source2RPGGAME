using System;
using System.Collections.Generic;
using Sandbox;

public static class DamageService
{
	public static readonly List<Func<GameObject, float, GameObject, float>> DamageModifiers = new();

	public static void ApplyDamage( GameObject target, float amount, GameObject launcher )
	{
		if ( !IsValidTarget( target, launcher ) ) return;

		float modified = amount;
		foreach ( var modifier in DamageModifiers )
			modified = modifier( target, modified, launcher );

		if ( modified <= 0f ) return;

		var health = target.GetHealth();
		if ( health != null )
			health.TakeDamage( modified, launcher );
	}

	public static bool IsValidTarget( GameObject target, GameObject launcher )
	{
		if ( target == null || !target.IsValid() ) return false;
		if ( target.IsOwnedBy( launcher ) ) return false;
		if ( !target.Tags.Has( GameTags.Enemy ) ) return false;
		return true;
	}
}

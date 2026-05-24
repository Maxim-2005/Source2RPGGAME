using Sandbox;

public static class DamageService
{
	public static void ApplyDamage( GameObject target, float amount, GameObject launcher )
	{
		if ( !IsValidTarget( target, launcher ) ) return;

		var health = target.GetHealth();
		if ( health != null )
			health.TakeDamage( amount, launcher );
	}

	public static bool IsValidTarget( GameObject target, GameObject launcher )
	{
		if ( target == null || !target.IsValid() ) return false;
		if ( target.IsOwnedBy( launcher ) ) return false;
		if ( !target.Tags.Has( GameTags.Enemy ) ) return false;
		return true;
	}
}

public static class GameObjectExtensions
{
	public static HealthComponent GetHealth( this GameObject gameObject )
	{
		return gameObject.Components.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors )
			   ?? gameObject.Components.Get<HealthComponent>( FindMode.EverythingInDescendants );
	}

	public static bool IsOwnedBy( this GameObject gameObject, GameObject owner )
	{
		if ( gameObject == null || owner == null ) return false;
		if ( gameObject == owner ) return true;
		if ( gameObject.Root == null || owner.Root == null ) return false;
		return gameObject.Root == owner.Root;
	}
}

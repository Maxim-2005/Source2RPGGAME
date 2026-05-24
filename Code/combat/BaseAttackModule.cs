using System;
using System.Threading.Tasks;
using Sandbox;

public abstract class BaseAttackModule : Component
{
	public bool IsAttacking { get; protected set; } = false;

	public abstract bool TryAttack( GameObject attacker, SkinnedModelRenderer playerModel );
	public abstract void StopAttack();

	protected async Task RunAttackAsync( float preAttackDelay, SkinnedModelRenderer playerModel, string animationName, Func<Task> attackBody )
	{
		try
		{
			IsAttacking = true;
			if ( playerModel != null ) playerModel.Set( animationName, true );
			await Task.DelaySeconds( preAttackDelay );
			if ( !IsValid || !GameObject.IsValid() || GameObject.Parent == null )
			{
				IsAttacking = false;
				return;
			}
			await attackBody();
		}
		catch ( Exception e )
		{
			Log.Error( e );
		}
		finally
		{
			IsAttacking = false;
		}
	}
}

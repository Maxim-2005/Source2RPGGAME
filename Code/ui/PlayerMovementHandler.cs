public sealed class PlayerMovementHandler : Component
{
	[Property] public float StaminaRestoreThreshold { get; set; } = 0.3f;

	private PlayerStats _stats;
	private PlayerController _controller;
	private float _originalRunSpeed;
	private bool _speedRestricted;

	protected override void OnStart()
	{
		_stats = Components.Get<PlayerStats>();
		_controller = Components.Get<PlayerController>();
		_originalRunSpeed = _controller?.RunSpeed ?? 300f;
	}

	protected override void OnUpdate()
	{
		if ( _stats is null || _controller is null ) return;

		if ( _stats.Stamina <= 0f )
		{
			_speedRestricted = true;
		}
		else if ( _stats.Stamina >= _stats.MaxStamina * StaminaRestoreThreshold )
		{
			_speedRestricted = false;
		}

		_controller.RunSpeed = _speedRestricted ? _controller.WalkSpeed : _originalRunSpeed;
	}
}

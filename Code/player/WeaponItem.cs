using Sandbox;

public sealed class WeaponItem : Component
{
	[Property, Group( "Hold Settings" )] public Vector3 HoldOffset { get; set; }
	[Property, Group( "Hold Settings" )] public Rotation HoldRotation { get; set; }
	[Property, Group( "Hold Settings" )] public int HoldType { get; set; } = 4;

	[Property, Group( "Throw Settings" )] public float ThrowForce { get; set; } = 400f;

	private SkinnedModelRenderer _playerModel;
	private string _attachmentName = "";
	private bool _isHeld = false;
	private ModelRenderer _itemRenderer;

	// Ссылка на модуль атаки (лук, посох, меч — не важно)
	private BaseAttackModule _attackModule;

	protected override void OnStart()
	{
		_itemRenderer = GameObject.Components.Get<ModelRenderer>();

		// Автоматически ищем любой боевой модуль на этом объекте
		_attackModule = GameObject.Components.Get<BaseAttackModule>( FindMode.EverythingInSelfAndChildren );

		if ( _itemRenderer == null )
		{
			Log.Warning( $"На объекте {GameObject.Name} нет ModelRenderer!" );
		}
	}

	protected override void OnUpdate()
	{
		// ЕСЛИ ПРЕДМЕТ В РУКАХ
		if ( _isHeld && _playerModel != null )
		{
			var att = _playerModel.GetAttachment( _attachmentName );
			if ( att.HasValue )
			{
				GameObject.WorldPosition = att.Value.Position + (att.Value.Rotation * HoldOffset);
				GameObject.WorldRotation = att.Value.Rotation * HoldRotation;
			}

			// УНИВЕРСАЛЬНАЯ ЛОГИКА АТАКИ:
			if ( _attackModule != null )
			{
				// Если ЛКМ зажата/нажата (модуль сам решит, как реагировать)
				if ( Input.Down( "attack1" ) )
				{
					_attackModule.TryAttack( GameObject.Parent, _playerModel );
				}
				else
				{
					// Если ЛКМ отпустили (нужно для луков или отмены каста)
					_attackModule.StopAttack();
				}
			}

			// Проверка выбрасывания на клавишу G
			bool isBusy = _attackModule != null && _attackModule.IsAttacking;
			if ( Input.Keyboard.Down( "G" ) && !isBusy )
			{
				Drop();
			}

			return;
		}

		// ЕСЛИ ПРЕДМЕТ НА ПОЛУ
		var players = Scene.GetAll<PlayerController>();
		foreach ( var player in players )
		{
			if ( Vector3.DistanceBetween( GameObject.WorldPosition, player.GameObject.WorldPosition ) < 100 && Input.Keyboard.Down( "E" ) )
			{
				Pickup( player );
				break;
			}
		}
	}

	public void Pickup( PlayerController player )
	{
		_playerModel = player.Components.GetInChildren<SkinnedModelRenderer>();
		if ( _playerModel == null ) return;

		_attachmentName = "right_hand";
		if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "r_hand";
		if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "hand_r";
		if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "weapon_r";

		_isHeld = true;
		GameObject.Parent = player.GameObject;

		var rb = GameObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );
		if ( rb != null ) rb.Enabled = false;

		GameObject.Tags.Add( "trigger" );

		_playerModel.Set( "holdtype", HoldType );
		_playerModel.Set( "holdtype_handedness", 1 );
	}

	public void Drop()
	{
		if ( _attackModule != null ) _attackModule.StopAttack();

		Vector3 throwDir = GameObject.WorldRotation.Forward;
		Vector3 dropPosition = GameObject.WorldPosition;
		Rotation dropRotation = GameObject.WorldRotation;

		if ( _playerModel != null )
		{
			throwDir = _playerModel.WorldRotation.Forward;
			dropPosition = _playerModel.WorldPosition + Vector3.Up * 45f;
			_playerModel.Set( "holdtype", 0 );
		}

		GameObject.Parent = null;
		GameObject.WorldPosition = dropPosition;
		GameObject.WorldRotation = dropRotation;
		GameObject.Tags.Remove( "trigger" );

		_isHeld = false;
		_playerModel = null;
		_attachmentName = "";

		var rb = GameObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );
		if ( rb == null ) rb = GameObject.AddComponent<Rigidbody>();

		rb.Enabled = true;
		rb.Gravity = true;
		rb.Velocity = throwDir * ThrowForce;
	}
}

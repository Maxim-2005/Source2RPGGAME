using Sandbox;

public sealed class AxePickupTest : Component
{
	[Property] public string AxeModel { get; set; } = "axe.vmdl";
	private SkinnedModelRenderer _playerModel;
	private string _attachmentName = "";
	private bool _isHeld = false;

	[Property] public Vector3 HoldOffset { get; set; }
	[Property] public Rotation HoldRotation { get; set; }
	[Property] public float ThrowForce { get; set; } = 800f;
	[Property] public float SpinForce { get; set; } = 3000f;

	protected override void OnStart()
	{
		HoldOffset = new Vector3( 5.5f, 1.5f, -1.5f );
		HoldRotation = Rotation.From( 20, 4, -80 );

		var mr = GameObject.AddComponent<ModelRenderer>();
		mr.Model = Model.Load( AxeModel ) ?? Model.Load( "models/dev/box.vmdl" );
	}


	void Attack()
	{
		if ( _playerModel == null ) return;

		// Отправляем триггер атаки в Animgraph
		_playerModel.Set( "b_attack", true );
	}

	protected override void OnUpdate()
	{
		// 1. Атака срабатывает ТОЛЬКО если топор в руках (_isHeld == true)
		if ( _isHeld && Input.Pressed( "attack1" ) )
		{
			Attack();
		}

		if ( Input.Keyboard.Down( "G" ) && _isHeld == true )
		{
			Drop();
		}

		if ( _isHeld && _playerModel != null )
		{
			var att = _playerModel.GetAttachment( _attachmentName );
			if ( att.HasValue )
			{
				GameObject.WorldPosition = att.Value.Position + (att.Value.Rotation * HoldOffset);
				GameObject.WorldRotation = att.Value.Rotation * HoldRotation;
			}
			return; // Если предмет в руках, прерываем проверку на подбор
		}

		var players = Scene.GetAll<PlayerController>();
		foreach ( var player in players )
		{
			if ( Vector3.DistanceBetween( GameObject.WorldPosition, player.GameObject.WorldPosition ) < 100 && Input.Keyboard.Down( "E" ) )
			{
				_playerModel = player.Components.GetInChildren<SkinnedModelRenderer>();
				if ( _playerModel != null )
				{
					_attachmentName = "right_hand";
					if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "r_hand";
					if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "hand_r";
					if ( !_playerModel.GetAttachment( _attachmentName ).HasValue ) _attachmentName = "weapon_r";

					_isHeld = true;

					// Устанавливаем holdtype для ближнего боя (4 - стандартный melee holdtype для Citizen)
					_playerModel.Set( "holdtype", 4 );
					_playerModel.Set( "holdtype_handedness", 1 ); // Оружие в правой руке
				}
				break;
			}
		}
	}

	public void Drop()
	{
		if ( !_isHeld ) return;
		Vector3 throwDir = GameObject.WorldRotation.Forward;

		if ( _playerModel != null )
		{
			throwDir = _playerModel.WorldRotation.Forward;

			// Сбрасываем holdtype при выбрасывании, чтобы персонаж опустил руки
			_playerModel.Set( "holdtype", 0 );
		}

		_isHeld = false;
		_playerModel = null;
		_attachmentName = "";
		GameObject.Parent = null;

		var rb = GameObject.Components.Get<Rigidbody>();
		if ( rb == null )
		{
			rb = GameObject.AddComponent<Rigidbody>();
		}
		rb.Enabled = true;
		rb.Gravity = true;
		rb.Velocity = throwDir * ThrowForce * 0.1f;
	}
}

using Sandbox;

public sealed class WeaponItem : Component
{
	[Property, Group( "Hold Settings" )] public Vector3 HoldOffset { get; set; }
	[Property, Group( "Hold Settings" )] public Rotation HoldRotation { get; set; }
	[Property, Group( "Hold Settings" )] public int HoldType { get; set; } = 4;

	[Property, Group( "Throw Settings" )] public float ThrowForce { get; set; } = 400f;

	[Hide] private SkinnedModelRenderer _playerModel;
	[Hide] private GameObject _playerObject;
	[Hide] private string _attachmentName = "";
	[Hide] private bool _isHeld = false;
	public bool IsHeld => _isHeld;
	[Hide] private ModelRenderer _itemRenderer;

	[Hide] private BaseAttackModule _attackModule;

	protected override void OnStart()
	{
		_itemRenderer = GameObject.Components.Get<ModelRenderer>();

		// ������������� ���� ����� ������ ������ �� ���� �������
		_attackModule = GameObject.Components.Get<BaseAttackModule>( FindMode.EverythingInSelfAndChildren );

		if ( _itemRenderer == null )
		{
			Log.Warning( $"Object {GameObject.Name} has no ModelRenderer!" );
		}
	}

	protected override void OnUpdate()
	{
		// ���� ������� � �����
		if ( _isHeld && _playerModel != null )
		{
			// ⠀��� ������ ��������� � ����� — ������ � GetAttachment ��� �����,
			// ��� ��� ��� ���������� ����� ������ ���������� ������������
			if ( GameObject.Parent != _playerModel.GetBoneObject( _attachmentName ) )
			{
				var att = _playerModel.GetAttachment( _attachmentName );
				if ( att.HasValue )
				{
					GameObject.WorldPosition = att.Value.Position + (att.Value.Rotation * HoldOffset);
					GameObject.WorldRotation = att.Value.Rotation * HoldRotation;
				}
			}

			// ������������� ������ �����:
			if ( _attackModule != null )
			{
				// ���� ��� ������/������ (������ ��� �����, ��� �����������)
				if ( Input.Down( "attack1" ) )
				{
					_attackModule.TryAttack( _playerObject, _playerModel );
				}
				else
				{
					// ���� ��� ��������� (����� ��� ����� ��� ������ �����)
					_attackModule.StopAttack();
				}
			}

			// �������� ������������ �� ������� G
			bool isBusy = _attackModule != null && _attackModule.IsAttacking;
			if ( Input.Keyboard.Down( "G" ) && !isBusy )
			{
				Drop();
			}

			return;
		}

		// ���� ������� �� ����
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

		_isHeld = true;
		_playerObject = player.GameObject;
		_playerModel.CreateBoneObjects = true;

		// сначала ищем кость (bone)
		string[] boneNames = { "hand_R", "hand_L", "right_hand", "r_hand", "hand_r", "weapon_r", "weapon_R" };
		foreach ( var name in boneNames )
		{
			var bone = _playerModel.GetBoneObject( name );
			if ( bone.IsValid() )
			{
				GameObject.Parent = bone;
				GameObject.LocalPosition = HoldOffset;
				GameObject.LocalRotation = HoldRotation;
				Log.Info( $"Weapon parented to bone: {name}" );
				goto done;
			}
		}

		// если кость не нашли — ищем аттачмент
		string[] attachNames = { "right_hand", "r_hand", "hand_r", "hand_R", "weapon_r", "Weapon", "RightHand", "RH", "righthand" };
		foreach ( var name in attachNames )
		{
			var att = _playerModel.GetAttachment( name );
			if ( att.HasValue )
			{
				_attachmentName = name;
				GameObject.Parent = player.GameObject;
				Log.Info( $"Weapon using attachment: {name}" );
				goto done;
			}
		}

		// ничего не нашли — цепляем к игроку как есть
		GameObject.Parent = player.GameObject;
		Log.Warning( "No suitable bone or attachment found for weapon!" );

		done:

		var rb = GameObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );
		if ( rb != null ) rb.Enabled = false;

		GameObject.Tags.Add( GameTags.Trigger );

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
		GameObject.Tags.Remove( GameTags.Trigger );

		_isHeld = false;
		_playerObject = null;
		_playerModel = null;
		_attachmentName = "";

		var rb = GameObject.Components.Get<Rigidbody>( FindMode.EverythingInSelfAndChildren );
		if ( rb == null ) rb = GameObject.AddComponent<Rigidbody>();

		rb.Enabled = true;
		rb.Gravity = true;
		rb.Velocity = throwDir * ThrowForce;
	}
}

using Sandbox;

public sealed class PlayerAnimator : Component
{
    private SkinnedModelRenderer _model;
    private PlayerController _controller;
    private string _currentAnim;
    private bool _isAttacking;

    protected override void OnStart()
    {
        _model = Components.GetInChildren<SkinnedModelRenderer>();
        _controller = Components.Get<PlayerController>();
        _model.UseAnimGraph = false;
        _model.Sequence.Blending = true;
        _model.Sequence.Looping = true;
        _model.Sequence.Name = "PLAYER_IDLE";
        _currentAnim = "PLAYER_IDLE";
    }

    protected override void OnUpdate()
    {
        bool hasWeapon = false;
        foreach ( var w in Scene.GetAll<WeaponItem>() )
            if ( w.IsHeld ) { hasWeapon = true; break; }

        if (_isAttacking)
        {
            if (_model.Sequence.IsFinished)
            {
                _isAttacking = false;
                _model.Sequence.Looping = true;
            }
        }

        if (!_isAttacking && hasWeapon && Input.Down("attack1"))
        {
            _isAttacking = true;
            _model.Sequence.Blending = true;
            _model.Sequence.Looping = false;
            _model.Sequence.Name = "PLAYER_SWORD_HIT";
            _currentAnim = "PLAYER_SWORD_HIT";
            return;
        }

        string desiredAnim;

        if (!_controller.IsOnGround)
            desiredAnim = _controller.Velocity.z > 0 ? "PLAYER_JUMP" : "PLAYER_FALL";
        else if (_controller.Velocity.Length > 200f)
            desiredAnim = "PLAYER_RUN";
        else if (_controller.Velocity.Length > 10f)
            desiredAnim = _controller.IsDucking ? "PLAYER_CROUCH_WALK" : "PLAYER_WALK";
        else
            desiredAnim = _controller.IsDucking ? "PLAYER_CROUCH_IDLE" : (hasWeapon ? "PLAYER_SWORD_HOLD" : "PLAYER_IDLE");

        if (!_isAttacking && _currentAnim != desiredAnim)
        {
            _currentAnim = desiredAnim;
            _model.Sequence.Blending = true;
            _model.Sequence.Looping = true;
            _model.Sequence.Name = desiredAnim;
        }
    }
}

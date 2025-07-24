public partial class UiModule : Sprite2D
{

    private const float StretchAmountPerPixel = 0.02f;
    private const float TargetPositionFollowSpeed = 10f;
    private const float CursorFollowSpeed = 30f;

    private static readonly StringName StretchAmount = "stretch_amount";
    private static readonly StringName StretchDirection = "stretch_direction";

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private bool _isFollowingCursor;
    private ShaderMaterial _material = null!;
    private Vector2 _targetPosition;

    public Module Module { get; private set; } = null!;
    public InventorySlot? Slot { get; set; }

    public static UiModule Create(Module module)
    {
        var uiModule = Scene.Instantiate<UiModule>();
        uiModule.Module = module;
        return uiModule;
    }

    public override void _Ready()
    {
        Texture = Module.Texture;
        _material = (ShaderMaterial)Material;
    }

    public override void _Process(double delta)
    {
        if (_isFollowingCursor)
        {
            _targetPosition = GetGlobalMousePosition();
        }
        else if (Slot != null)
        {
            _targetPosition = Slot.GetGlobalRect().GetCenter();
        }

        MoveToTargetPosition(delta);
    }

    public void MoveToSlotInstantly()
    {
        if (Slot != null)
        {
            _targetPosition = Slot.GetGlobalRect().GetCenter();
            GlobalPosition = _targetPosition;
        }
    }

    public void StartFollowingCursor()
    {
        _isFollowingCursor = true;
        ZIndex += 1;
    }

    public void StopFollowingCursor()
    {
        _isFollowingCursor = false;
        ZIndex -= 1;
    }

    public void SlotHovered()
    {
    }

    public void SlotUnhovered()
    {
    }

    private void MoveToTargetPosition(double delta)
    {
        var position = GlobalPosition;
        if (position.IsEqualApprox(_targetPosition))
        {
            return;
        }

        SetStretchDirection(position.DirectionTo(_targetPosition).Rotated(Pi / 2));

        var followSpeed = _isFollowingCursor ? CursorFollowSpeed : TargetPositionFollowSpeed;
        var distanceToX = Abs(position.X - _targetPosition.X);
        var distanceToY = Abs(position.Y - _targetPosition.Y);
        var x = MoveToward(
            position.X,
            _targetPosition.X,
            followSpeed * (float)delta * distanceToX
        );
        var y = MoveToward(
            position.Y,
            _targetPosition.Y,
            followSpeed * (float)delta * distanceToY
        );
        GlobalPosition = new Vector2(x, y);

        var positionDelta = GlobalPosition.DistanceTo(position);
        var stretchAmount = Clamp(StretchAmountPerPixel * positionDelta, 0, 2);
        SetStretchAmount(stretchAmount);
    }

    private void SetStretchAmount(float stretchAmount)
    {
        _material.SetShaderParameter(StretchAmount, stretchAmount);
    }

    private float GetStretchAmount()
    {
        return (float)_material.GetShaderParameter(StretchAmount);
    }

    private void SetStretchDirection(Vector2 stretchDirection)
    {
        _material.SetShaderParameter(StretchDirection, stretchDirection);
    }

    private Vector2 GetStretchDirection()
    {
        return (Vector2)_material.GetShaderParameter(StretchDirection);
    }

}
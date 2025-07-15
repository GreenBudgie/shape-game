public partial class UiModule : Sprite2D
{

    private const float MaxRot = 10;
    private const float MinRot = 5;
    private const float RotDistance = 100;

    private static readonly StringName YRot = "y_rot";
    private static readonly StringName XRot = "x_rot";
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private Vector2 _initialCursorPosition;
    private bool _isFollowingCursor;
    private ShaderMaterial _material = null!;

    public Module Module { get; private set; } = null!;
    public bool IsSlotHovered { get; set; }
    
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
            Position = GetViewport().GetMousePosition() - _initialCursorPosition;
        }

        if (IsSlotHovered)
        {
            var mouseDelta = GetGlobalMousePosition() - GlobalPosition;
            var mouseDeltaRatio = mouseDelta / RotDistance;
            var mouseDeltaRot = new Vector2(
                -mouseDeltaRatio.Y,
                mouseDeltaRatio.X
            ) * MaxRot;
            
            SetRot(mouseDeltaRot);
        }
        else
        {
            SetRot(Vector2.Zero);
        }
    }

    public void StartFollowingCursor()
    {
        var mousePosition = GetViewport().GetMousePosition();
        _initialCursorPosition = new Vector2(mousePosition.X, mousePosition.Y);
        _isFollowingCursor = true;
    }

    public void StopFollowingCursor()
    {
        Position = Vector2.Zero;
        _isFollowingCursor = false;
    }

    private void SetRot(Vector2 rot)
    {
        SetXRot(rot.X);
        SetYRot(rot.Y);
    }

    private Vector2 GetRot()
    {
        return new Vector2(GetXRot(), GetYRot());
    }

    private void SetXRot(float xRot)
    {
        _material.SetShaderParameter(XRot, xRot);
    }
    
    private void SetYRot(float yRot)
    {
        _material.SetShaderParameter(YRot, yRot);
    }
    
    private float GetXRot()
    {
        return (float)_material.GetShaderParameter(XRot);
    }
    
    private float GetYRot()
    {
        return (float)_material.GetShaderParameter(YRot);
    }
}
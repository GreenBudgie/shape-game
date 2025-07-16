public partial class UiModule : Sprite2D
{

    private const float MaxStretch = 10;
    private const float MaxStretchDistance = 100;

    private static readonly StringName StretchAmount = "stretch_amount";
    private static readonly StringName StretchDirection = "stretch_direction";
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://csoad8g8f13qn");

    private bool _isFollowingCursor;
    private ShaderMaterial _material = null!;

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
            GlobalPosition = GetGlobalMousePosition();
        }
    }

    public void StartFollowingCursor()
    {
        _isFollowingCursor = true;
    }

    public void StopFollowingCursor()
    {
        _isFollowingCursor = false;
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
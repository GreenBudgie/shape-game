public partial class Player : CharacterBody2D
{

    private const int CornerDistance = 28;

    private static Player? _instance;

    /**
     * The player's tilt will never go above this value.
     */
    [ExportGroup("Tilt")] [Export(PropertyHint.Range, "0,40,1,or_greater")]
    private double _maxTiltDegrees = 22;

    /**
     * How rapidly the player will tilt when it is moved horizontally. Higher = faster.
     */
    [Export(PropertyHint.Range, "0,1,0.01,or_greater")]
    private double _tiltIncreaseFactor = 0.1;

    /**
     * How fast the player's tilt will decrease over time (arbitrary number). Higher = faster.
     */
    [Export(PropertyHint.Range, "1,20,0.1,or_greater")]
    private double _tiltDecreaseFactor = 7;

    /**
     * The lowest threshold of player's tilt in degrees. When this value is reached, the tilt is instantly set to 0.
     */
    [Export(PropertyHint.Range, "0.1,2,0.1,or_greater")]
    private double _rotationDegreesEpsilon = 0.3;

    private Vector2 _windowCenter;
    private Blaster _leftBlaster = null!;

    public static Player? FindPlayer()
    {
        return _instance;
    }

    public override void _EnterTree()
    {
        _instance = this;
    }

    public override void _Ready()
    {
        _leftBlaster = new Blaster();
        AddChild(_leftBlaster);
        _windowCenter = new Vector2(
            GetViewportRect().Size.X / 2.0f,
            GetViewportRect().Size.Y / 2.0f
        );

        PauseManager.Instance.GameUnpause += () => MoveMouseToWindowCenter(_windowCenter);
        PauseManager.Instance.GamePause += () => MoveMouseToWindowCenter(ShapeGame.PlayableArea.GetCenter());
        InventoryManager.Instance.InventoryClosed += () => MoveMouseToWindowCenter(_windowCenter);
        InventoryManager.Instance.InventoryOpened += () => MoveMouseToWindowCenter(ShapeGame.PlayableArea.GetCenter());
    }

    public override void _Process(double delta)
    {
        var mouseDelta = Vector2.Zero;
        if (!InventoryManager.Instance.IsOpen)
        {
            mouseDelta = MoveMouseToWindowCenter(_windowCenter);
        }

        var prevPosition = Position;
        Velocity = mouseDelta * (float)(1 / delta);
        MoveAndSlide();

        var positionDelta = Position - prevPosition;
        var tiltDegrees = positionDelta.X * _tiltIncreaseFactor;
        RotationDegrees *= Clamp(1 - (float)(delta * _tiltDecreaseFactor), 0, 1);
        RotationDegrees = (float)Clamp(RotationDegrees + tiltDegrees, -_maxTiltDegrees, _maxTiltDegrees);
        if (Abs(RotationDegrees) <= _rotationDegreesEpsilon)
        {
            RotationDegrees = 0;
        }

        if (!InventoryManager.Instance.IsOpen && (int)Input.GetActionStrength("primary_fire") == 1)
        {
            PrimaryFire();
        }
    }

    private Vector2 MoveMouseToWindowCenter(Vector2 windowCenterToUse)
    {
        var mousePosition = GetViewport().GetMousePosition();
        GetViewport().WarpMouse(windowCenterToUse);
        return mousePosition - windowCenterToUse;
    }

    public Vector2 GetGlobalNosePosition()
    {
        return ToGlobal(Position - new Vector2(0, CornerDistance).Rotated(Rotation));
    }

    public Vector2 GetGlobalLeftCornerPosition()
    {
        return ToGlobal(Position + new Vector2(-CornerDistance, CornerDistance).Rotated(Rotation));
    }

    public Vector2 GetGlobalRightCornerPosition()
    {
        return ToGlobal(Position + new Vector2(CornerDistance, CornerDistance).Rotated(Rotation));
    }

    private void PrimaryFire()
    {
        _leftBlaster.Trigger();
    }
}
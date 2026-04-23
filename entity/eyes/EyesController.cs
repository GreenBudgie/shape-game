using System;

public partial class EyesController : Node2D
{
    public Node2D EyeOwner { get; private set; } = null!;
    public Vector2? LookTarget { get; private set; }
    private HealthController? _ownerHealthController;

    [Export] public EyeTargetStrategy TargetStrategy { get; private set; } = new PlayerEyeTargetStrategy();

    /**
     * Where the eye should look when target hasn't been found
     */
    [Export]
    public EyeLookDirection FallbackLookDirection { get; private set; } = EyeLookDirection.Down;

    public override void _Ready()
    {
        EyeOwner = GetParent<Node2D>();
        _ownerHealthController = HealthController.GetHealthControllerIfExists(EyeOwner);
    }

    public override void _Process(double delta)
    {
        UpdateLookTarget();
    }

    public Vector2 GetFallbackLookDirection()
    {
        return FallbackLookDirection switch
        {
            EyeLookDirection.Up => Vector2.Up,
            EyeLookDirection.Down => Vector2.Down,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private void UpdateLookTarget()
    {
        LookTarget = TargetStrategy.GetTarget(this);
    }
    
}
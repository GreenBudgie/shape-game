using System;
using System.Collections.Generic;
using System.Linq;

public partial class EyesController : Node2D
{
    public Node2D EyeOwner { get; private set; } = null!;
    public Vector2? LookTarget { get; private set; }
    private HealthController? _ownerHealthController;

    private List<Eye> _eyes = null!;

    [Export] public EyeTargetStrategy TargetStrategy { get; private set; } = new PlayerEyeTargetStrategy();

    /**
     * Where the eye should look when target hasn't been found
     */
    [Export]
    public EyeLookDirection FallbackLookDirection { get; private set; } = EyeLookDirection.Down;

    public override void _Ready()
    {
        _eyes = GetChildren().Cast<Eye>().ToList();

        EyeOwner = GetParent<Node2D>();
        _ownerHealthController = HealthController.GetHealthControllerIfExists(EyeOwner);

        if (_ownerHealthController != null)
        {
            _ownerHealthController.Damaged += OnDamage;
            _ownerHealthController.Destroyed += OnDestroy;
        }
    }

    public override void _ExitTree()
    {
        if (_ownerHealthController == null)
        {
            return;
        }
        
        _ownerHealthController.Damaged -= OnDamage;
        _ownerHealthController.Destroyed -= OnDestroy;
    }

    private void OnDamage(float damage)
    {
        foreach (var eye in _eyes)
        {
            eye.SwitchTexture(EyeTextures.Dead);
        }
    }
    
    private void OnDestroy()
    {
        foreach (var eye in _eyes)
        {
            eye.SwitchTexture(EyeTextures.Dead);
        }
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
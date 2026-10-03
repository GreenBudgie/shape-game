using System;
using System.Collections.Generic;
using System.Linq;

public partial class EyesController : Node2D
{
    public Vector2? LookTarget { get; private set; }

    private List<Eye> _eyes = null!;

    [Export] public Entity EyeOwner { get; private set; } = null!;

    [Export] public EyeTargetStrategy TargetStrategy { get; private set; } = new PlayerEyeTargetStrategy();

    /**
     * Where the eye should look when target hasn't been found
     */
    [Export]
    public EyeLookDirection FallbackLookDirection { get; private set; } = EyeLookDirection.Down;

    public override void _Ready()
    {
        _eyes = GetChildren().Cast<Eye>().ToList();

        EyeOwner.HealthController.HealthChanged += OnHealthChange;
        EyeOwner.HealthController.Destroyed += OnDestroy;
    }

    public override void _ExitTree()
    {
        EyeOwner.HealthController.HealthChanged -= OnHealthChange;
        EyeOwner.HealthController.Destroyed -= OnDestroy;
    }

    private void OnHealthChange(float delta)
    {
        if (delta >= 0)
        {
            return;
        }

        foreach (var eye in _eyes)
        {
            eye.SwitchTexture(EyeTextures.Damaged);
            eye.Shake();
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
        GlobalPosition = EyeOwner.Sprite.GlobalPosition;
        GlobalRotation = EyeOwner.Sprite.GlobalRotation;
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
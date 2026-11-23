using System;

public partial class Laser : Node2D, ISpawnable
{

    private const float DelayBeforeImpact = 0.38f;
    
    private static readonly AudioStream FireSound = GD.Load<AudioStream>("uid://clklvhoo0gk4o");

    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;

    public bool IsRemoving { get; private set; }

    private Beam _beam = null!;
    private Tween? _tween;

    public override void _Ready()
    {
        if (Context.Source == null)
        {
            Remove();
            return;
        }

        GlobalPosition = GetSourcePosition();

        _beam = Beam.Create()
            .SetFromTo(GlobalPosition, GetTargetPosition())
            .SetEnergy(1)
            .SetProgress(0)
            .SetThickness(50)
            .SetOutlineThickness(200)
            .SetOutlineColor(ColorScheme.Red)
            .SetBeamColor(Colors.White);

        _tween = _beam.CreateTween().SetParallel();
        
        _tween.TweenProperty(_beam.ShaderMaterial, Beam.ProgressShaderParam, 1, DelayBeforeImpact)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        _tween.TweenProperty(_beam.ShaderMaterial, Beam.ThicknessShaderParam, 30, DelayBeforeImpact)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        _tween.TweenProperty(_beam.ShaderMaterial, Beam.OutlineThicknessShaderParam, 100, DelayBeforeImpact)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        
        _tween.Finished += PlayImpact;

        ShapeGame.Instance.AddChild(_beam);
    }

    public override void _Process(double delta)
    {
        GlobalPosition = GetSourcePosition();
        _beam.SetFromTo(GlobalPosition, GetTargetPosition());
    }

    public void Remove()
    {
        _beam.QueueFree();
        QueueFree();
        Removed?.Invoke();
    }

    private void PlayImpact()
    {
        ScreenShake.Instance.Shake(ShakeStrength.Low);
        
        var sound = SoundManager.Instance.PlayPositionalSound(this, FireSound);
        sound.RandomizePitchOffset(0.1f);
    }

    private Vector2 GetTargetPosition()
    {
        return new Vector2(GlobalPosition.X, ShapeGame.PlayableArea.Position.Y - 16);
    }

    private Vector2 _prevPosition = ShapeGame.Center;

    private Vector2 GetSourcePosition()
    {
        if (!IsInstanceValid(Context.Source))
        {
            return _prevPosition;
        }

        _prevPosition = Context.Source.GlobalPosition;
        return _prevPosition;
    }
}
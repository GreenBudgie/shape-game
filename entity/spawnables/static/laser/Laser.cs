using System;

public partial class Laser : Node2D, ISpawnable
{

    private const float DelayBeforeImpact = 0.33f;
    private const float RemoveDuration = 0.2f;
    
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
                
        var sound = SoundManager.Instance.PlayPositionalSound(this, FireSound);
        sound.RandomizePitchOffset(0.1f);

        _beam = Beam.Create();
        _beam.SetFromTo(GlobalPosition, GetTargetPosition());
        _beam.Energy = 0;
        _beam.Progress = 0;
        _beam.Thickness = 100;
        _beam.OutlineThickness = 300;
        _beam.OutlineColor = ColorScheme.Red;
        _beam.BeamColor = Colors.White;
        _beam.FadeDistance = 0.01f;
        _beam.Length = 0;
        _beam.BeamCount = 1;

        _tween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quart);

        _tween.TweenProperty(_beam, Beam.ProgressProperty, 1f, DelayBeforeImpact);
        _tween.TweenProperty(_beam, Beam.LengthProperty, 1f, DelayBeforeImpact);
        _tween.TweenProperty(_beam, Beam.EnergyProperty, 5f, DelayBeforeImpact);
        
        _tween.Finished += PlayImpact;

        ShapeGame.Instance.AddChild(_beam);
    }

    public override void _Process(double delta)
    {
        GlobalPosition = GetSourcePosition();
        _beam.SetFromTo(GlobalPosition, GetTargetPosition());
    }

    public override void _ExitTree()
    {
        if (IsInstanceValid(_beam))
        {
            _beam.QueueFree();
        }
    }

    public void Remove()
    {
        _tween?.Kill();
        _tween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _tween.TweenProperty(_beam, Beam.ProgressProperty, 0f, RemoveDuration);
        _tween.TweenProperty(_beam, Beam.LengthProperty, 0f, RemoveDuration);
        _tween.TweenProperty(_beam, Beam.EnergyProperty, 0f, RemoveDuration);
        
        _tween.Finished += QueueFree;
    }

    private void PlayImpact()
    {
        ScreenShake.Instance.Shake(ShakeStrength.Low);

        var impactDuration = Context.GetStat<LifetimeStat>() - DelayBeforeImpact;
        
        _tween?.Kill();
        _tween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _tween.TweenProperty(_beam, Beam.ThicknessProperty, 50, impactDuration);
        _tween.TweenProperty(_beam, Beam.OutlineThicknessProperty, 200, impactDuration);
        _tween.TweenProperty(_beam, Beam.EnergyProperty, 2f, impactDuration);
    }

    private Vector2 GetTargetPosition()
    {
        return new Vector2(GlobalPosition.X, ShapeGame.PlayableArea.Position.Y - 100);
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

using System.Collections.Generic;

public partial class TargetDisplay : Node2D
{

    private const float MaxDistanceFromCenter = 100f;
    private const float MinDistanceFromCenter = 50f;
    private const float MaxDeviationFromCenter = 25f;

    private static readonly Texture2D TargetPartTexture = GD.Load<Texture2D>("uid://b2tyclh65omag");

    public Entity? Target { get; private set; }
    
    private readonly List<TargetPart> _parts = [];
    private readonly Vector2 _randomDeviation;
    private bool _isRemoving = false;
    
    public TargetDisplay()
    {
        Modulate = Colors.Transparent;
        CreatePart(Vector2.Right);
        CreatePart(Vector2.Up);
        CreatePart(Vector2.Left);
        CreatePart(Vector2.Down);

        _randomDeviation = RandomUtils.RandomPointInRadius(MaxDeviationFromCenter);
        Rotation = RandomUtils.DeltaRange(0, Pi / 4);
    }

    private void CreatePart(Vector2 direction)
    {
        var sprite = new Sprite2D
        {
            Texture = TargetPartTexture,
            Rotation = direction.Angle(),
            Position = direction * MaxDistanceFromCenter
        };
        
        AddChild(sprite);
        _parts.Add(new TargetPart(sprite, direction));
    }

    private Tween? _tween;

    public void AttachToEntity(Entity entity)
    {
        if (_isRemoving)
        {
            return;
        }
        
        if (Target == null)
        {
            GlobalPosition = entity.GlobalPosition;
        }
        else
        {
            entity.HealthController.Destroyed -= OnTargetDestroyed;
        }
        
        Target = entity;
        entity.HealthController.Destroyed += OnTargetDestroyed;
        
        _tween?.Kill();
        _tween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Back).SetParallel();

        _tween.FadeIn(this, 0.5f).SetTrans(Tween.TransitionType.Quad);
        foreach (var part in _parts)
        {
            _tween.TweenPosition(part.Sprite, part.Direction * MinDistanceFromCenter, 0.75f);
        }
    }

    public override void _Process(double delta)
    {
        if (Target == null)
        {
            return;
        }

        if (!IsInstanceValid(Target))
        {
            Remove();
            return;
        }

        var targetPositionWithDeviation = Target.GlobalPosition + _randomDeviation;
        var distance = GlobalPosition.DistanceTo(targetPositionWithDeviation);

        const float followSpeedIncrease = 0.5f;
        const float maxVelocity = 1000;

        var speed = Min(maxVelocity, distance * followSpeedIncrease);

        GlobalPosition = GlobalPosition.MoveToward(targetPositionWithDeviation, speed);
    }

    private void OnTargetDestroyed()
    {
        Target = null;
        Remove();
    }

    public void Remove()
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isRemoving = true;
        
        _tween?.Kill();
        _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad).SetParallel();

        _tween.FadeOut(this, 0.5f);
        foreach (var part in _parts)
        {
            _tween.TweenPosition(part.Sprite, part.Direction * MaxDistanceFromCenter, 0.5f);
        }

        _tween.Finished += QueueFree;
    }

    private record TargetPart(Sprite2D Sprite, Vector2 Direction);

}
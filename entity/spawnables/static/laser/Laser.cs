using System;
using System.Collections.Generic;
using System.Linq;

public partial class Laser : Area2D, ISpawnable
{

    private const float DelayBeforeImpact = 0.33f;
    private const float RemoveDuration = 0.2f;
    private const float LaserWidth = 75;

    private static readonly float LaserLength = ShapeGame.WindowSize.X + 100;
    
    private static readonly AudioStream FireSound = GD.Load<AudioStream>("uid://clklvhoo0gk4o");

    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;

    public bool IsRemoving { get; private set; }

    private Beam _beam = null!;
    private Tween? _tween;
    private float _damage;
    private float _direction;
    private bool _registerDamage;
    private readonly HashSet<Entity> _damagedEntities = [];

    public Laser()
    {
        var rectangleShape = new RectangleShape2D();
        rectangleShape.Size = new Vector2(LaserWidth, LaserLength);

        var shape = new CollisionShape2D();
        shape.Position = new Vector2(0, -LaserLength / 2);
        shape.Shape = rectangleShape;
        
        AddChild(shape);
    }

    public override void _Ready()
    {
        if (Context.Source == null)
        {
            Remove();
            return;
        }

        _damage = Context.GetStat<DamageStat>();

        CollisionLayer = 0;
        CollisionMask = 0;
        
        if (Context.OriginalSource is not Enemy)
        {
            this.EnableCollisionMask(CollisionLayers.Enemies);
        }
        
        if (Context.OriginalSource is not Player)
        {
            this.EnableCollisionMask(CollisionLayers.Player);
        }

        GlobalPosition = GetSourcePosition();
        _direction = Context.Direction.Angle();
        Rotation = _direction + Pi / 2;
                
        var sound = SoundManager.Instance.PlayPositionalSound(this, FireSound);
        sound.RandomizePitchOffset(0.1f);

        _beam = Beam.Create();
        _beam.SetFromTo(GlobalPosition, GetTargetPosition());
        _beam.Energy = 0;
        _beam.Progress = 0;
        _beam.Thickness = 75;
        _beam.OutlineThickness = 200;
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

    public override void _PhysicsProcess(double delta)
    {
        if (!_registerDamage)
        {
            return;
        }
        
        Damage();
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
        _registerDamage = false;
        
        _tween?.Kill();
        _tween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _tween.TweenProperty(_beam, Beam.ProgressProperty, 0f, RemoveDuration);
        _tween.TweenProperty(_beam, Beam.LengthProperty, 0f, RemoveDuration);
        _tween.TweenProperty(_beam, Beam.EnergyProperty, 0f, RemoveDuration);
        
        _tween.Finished += QueueFree;
    }

    private void PlayImpact()
    {
        _registerDamage = true;
        
        ScreenShake.Instance.Shake(ShakeStrength.Low);

        var impactDuration = Context.GetStat<LifetimeStat>() - DelayBeforeImpact;
        
        _tween?.Kill();
        _tween = CreateTween().SetParallel().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);

        _tween.TweenProperty(_beam, Beam.ThicknessProperty, 40, impactDuration);
        _tween.TweenProperty(_beam, Beam.OutlineThicknessProperty, 150, impactDuration);
        _tween.TweenProperty(_beam, Beam.EnergyProperty, 2f, impactDuration);
    }

    private void Damage()
    {
        var entities = GetOverlappingBodies().OfType<Entity>().Where(entity => !_damagedEntities.Contains(entity));
        foreach (var entity in entities)
        {
            entity.HealthController.Damage(DamageSource.Explosion, _damage);
            var entityDamagedEvent = new EntityDamagedEvent(entity, _damage);
            EntityDamaged?.Invoke(entityDamagedEvent);

            _damagedEntities.Add(entity);

            const float impulseStrength = 1000f;
            entity.ApplyCentralImpulse(Vector2.Up * impulseStrength);
        }
    }

    private Vector2 GetTargetPosition()
    {
        return GlobalPosition + Vector2.FromAngle(_direction) * LaserLength;
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

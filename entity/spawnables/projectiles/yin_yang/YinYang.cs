using System;
using System.Collections.Generic;

public partial class YinYang : Node2D, ISpawnable
{
    private const float PathRadius = 128f;
    const float RotationSpeedMin = 7f;
    const float RotationSpeedMax = 9f;
    
    public event Action<EntityDamagedEvent>? EntityDamaged;
    public event Action? Removed;
    public SpawnableContext Context { get; set; } = null!;

    public bool IsRemoving { get; private set; }

    private Node2D _yinFollowTarget = null!;
    private Node2D _yangFollowTarget = null!;
    private YinYangSphere _yinSphere = null!;
    private YinYangSphere _yangSphere = null!;

    private float _yinRotation;
    private float _yangRotation = Pi;
    
    private Vector2 _direction;
    private float _speed;
    private float _rotationSpeed;

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cfienje1goost");

    public static YinYang Create()
    {
        var node = Scene.Instantiate<YinYang>();
        return node;
    }

    public bool IsProxy => true;

    public override void _Ready()
    {
        _direction = Context.Direction;
        _speed = Context.GetStat<SpeedStat>();
        _rotationSpeed = (float)GD.RandRange(RotationSpeedMin, RotationSpeedMax) * RandomUtils.RandomSign();

        _yinFollowTarget = GetNode<Node2D>("YinFollowTarget");
        _yangFollowTarget = GetNode<Node2D>("YangFollowTarget");

        var yinSphereContext = CreateSphereContext(YinYangType.Yin, _yinFollowTarget);
        var yangSphereContext = CreateSphereContext(YinYangType.Yang, _yangFollowTarget);
        
        _yinSphere = (YinYangSphere)yinSphereContext.Spawnable;
        _yangSphere = (YinYangSphere)yangSphereContext.Spawnable;

        _yinSphere.OtherSphere = _yangSphere;
        _yangSphere.OtherSphere = _yinSphere;

        _yinSphere.TreeExiting += Remove;
        _yangSphere.TreeExiting += Remove;

        yinSphereContext.Spawn();
        yangSphereContext.Spawn();
    }

    private SpawnableContext CreateSphereContext(YinYangType type, Node2D followTarget)
    {
        var producer = new YinYangSphereProducer(Context.SpawnableProducer, type, followTarget);
        
        var context = Context.Clone(producer);
        context.Position = followTarget.GlobalPosition;
        context.CalculateStats();
        context.CreateSpawnable();
        return context;
    }

    public override void _Process(double delta)
    {
        _yinRotation = (_yinRotation + _rotationSpeed * (float)delta) % Tau;
        _yangRotation = (_yangRotation + _rotationSpeed * (float)delta) % Tau;

        _yinFollowTarget.Position = Vector2.FromAngle(_yinRotation) * PathRadius;
        _yangFollowTarget.Position = Vector2.FromAngle(_yangRotation) * PathRadius;

        GlobalPosition += (float)delta * _speed * _direction;
    }

    public void Remove()
    {
        if (IsRemoving)
        {
            return;
        }
        
        IsRemoving = true;

        _yinSphere.Detach();
        _yangSphere.Detach();
        
        QueueFree();
        Removed?.Invoke();
    }

    private class YinYangSphereProducer(
        ISpawnableProducer parentProducer,
        YinYangType type,
        Node2D followTarget
    ) : ISpawnableProducer
    {
        public List<SpawnableStat> GetStats() => parentProducer.GetStats();

        public ISpawnable CreateSpawnable()
        {
            return YinYangSphere.Create(type, followTarget);
        }
    }
}
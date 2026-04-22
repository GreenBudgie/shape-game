using System;

public partial class Eye : Node2D
{

    private const float EyeballSize = 20;
    private const float PupilSize = 10;
    private const float Gap = 4;

    private const float DistanceFromCenter = EyeballSize - PupilSize - Gap;

    private Node2D _owner = null!;

    private Node2D _followTarget = null!;
    private Vector2? _lookTarget;
    
    private Sprite2D _eyeball = null!;
    private Sprite2D _pupil = null!;

    [Export] private EyeTargetStrategy _targetStrategy = new PlayerEyeTargetStrategy();
    
    /**
     * Where the eye should look when target hasn't been found
     */
    [Export] private EyeLookDirection _fallbackLookDirection = EyeLookDirection.Down;
    
    public override void _Ready()
    {
        _owner = GetParent<Node2D>();
        
        Callable.From(Setup).CallDeferred();
        
        _eyeball = GetNode<Sprite2D>("Eyeball");
        _pupil = GetNode<Sprite2D>("Pupil");

        _owner.TreeExiting += QueueFree;
    }

    private void Setup()
    {
        _followTarget = new Node2D();
        _owner.AddChild(_followTarget);
        _followTarget.GlobalPosition = GlobalPosition;

        Reparent(ShapeGame.Instance);
        ZIndex = _owner.ZIndex + 1;
    }

    public override void _Process(double delta)
    {
        DoFollowTarget();
        UpdateLookTarget();
        DoMovePupil();
    }

    private float _velocity;

    private void DoFollowTarget()
    {
        const float velocityDecreaseFactor = 0.85f;
        _velocity *= velocityDecreaseFactor;

        var distance = GlobalPosition.DistanceTo(_followTarget.GlobalPosition);

        const float followSpeedIncrease = 0.1f;
        const float maxVelocity = 1000;

        var increase = Min(maxVelocity, distance * followSpeedIncrease);
        _velocity += increase;

        GlobalPosition = GlobalPosition.MoveToward(_followTarget.GlobalPosition, _velocity);
    }

    private void DoMovePupil()
    {
        Vector2 direction;
        if (_lookTarget.HasValue)
        {
            direction = _pupil.GlobalPosition.DirectionTo(_lookTarget.Value);
        }
        else
        {
            direction = _fallbackLookDirection switch
            {
                EyeLookDirection.Up => Vector2.Up,
                EyeLookDirection.Down => Vector2.Down,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        const float moveSpeed = 0.3f;
        var position = direction * DistanceFromCenter;
        _pupil.Position = _pupil.Position.MoveToward(position, moveSpeed);
    }

    private void UpdateLookTarget()
    {
        _lookTarget = _targetStrategy.GetTarget(this, _owner);
    }
    
}

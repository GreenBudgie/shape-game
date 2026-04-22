public partial class Eye : Node2D
{

    private const float EyeballSize = 20;
    private const float PupilSize = 10;
    private const float Gap = 4;

    private const float DistanceFromCenter = EyeballSize - PupilSize - Gap;

    private Node2D _entity = null!;

    private Node2D _followTarget = null!;
    private Vector2? _lookTarget;
    
    private Sprite2D _eyeball = null!;
    private Sprite2D _pupil = null!;
    
    public override void _Ready()
    {
        _entity = GetParent<Node2D>();
        
        _followTarget = new Node2D();
        _followTarget.GlobalPosition = GlobalPosition;
        _entity.AddChildDeferred(_followTarget);
        
        Callable.From(() => Reparent(ShapeGame.Instance)).CallDeferred();
        
        _eyeball = GetNode<Sprite2D>("Eyeball");
        _pupil = GetNode<Sprite2D>("Pupil");

        _entity.TreeExiting += QueueFree;
    }

    public override void _Process(double delta)
    {
        DoFollowTarget();

        var player = Player.FindPlayer();
        if (player != null)
        {
            SetTarget(player.GlobalPosition);
        }
        
        if (!_lookTarget.HasValue)
        {
            return;
        }

        var direction = _pupil.GlobalPosition.DirectionTo(_lookTarget.Value);
        var position = direction * DistanceFromCenter;
        _pupil.Position = position;
    }

    private float _velocity;

    private void DoFollowTarget()
    {
        const float minVelocity = 1f;
        const float velocityDecreaseFactor = 1.5f;
        _velocity /= velocityDecreaseFactor;

        const float minDistanceSq = 1f;
        var distanceSq = GlobalPosition.DistanceSquaredTo(_followTarget.GlobalPosition);
        
        if (_velocity <= minVelocity)
        {
            _velocity = 0;

            if (distanceSq <= minDistanceSq)
            {
                GlobalPosition = _followTarget.GlobalPosition;
                return;
            }
        }

        const float followSpeedIncrease = 0.01f;
        _velocity += followSpeedIncrease * distanceSq;
        
        GlobalPosition.MoveToward(_followTarget.GlobalPosition, _velocity);
    }

    public void SetTarget(Vector2 globalPosition)
    {
        _lookTarget = globalPosition;
    }
    
}

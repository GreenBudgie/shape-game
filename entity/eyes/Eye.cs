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
        
        Callable.From(Setup).CallDeferred();
        
        _eyeball = GetNode<Sprite2D>("Eyeball");
        _pupil = GetNode<Sprite2D>("Pupil");

        _entity.TreeExiting += QueueFree;
    }

    private void Setup()
    {
        _followTarget = new Node2D();
        _entity.AddChild(_followTarget);
        _followTarget.GlobalPosition = GlobalPosition;

        Reparent(ShapeGame.Instance);
        ZIndex = _entity.ZIndex + 1;
    }

    public override void _Process(double delta)
    {
        DoFollowTarget();

        var player = Player.FindPlayer();
        if (player != null)
        {
            SetTarget(player.GlobalPosition);
        }
        
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
        if (!_lookTarget.HasValue)
        {
            return;
        }

        const float moveSpeed = 0.3f;
 
        var direction = _pupil.GlobalPosition.DirectionTo(_lookTarget.Value);
        var position = direction * DistanceFromCenter;
        _pupil.Position = _pupil.Position.MoveToward(position, moveSpeed);
    }

    public void SetTarget(Vector2 globalPosition)
    {
        _lookTarget = globalPosition;
    }
    
}

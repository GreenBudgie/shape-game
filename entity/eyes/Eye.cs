public partial class Eye : Node2D
{

    private const float EyeballSize = 20;
    private const float PupilSize = 10;
    private const float Gap = 4;

    private const float DistanceFromCenter = EyeballSize - PupilSize - Gap;

    private EyesController _controller = null!;

    private Node2D _followTarget = null!;
    
    private Sprite2D _eyeball = null!;
    private Sprite2D _pupil = null!;

    private double _textureSwitchTimer;
    
    public override void _Ready()
    {
        _controller = GetParent<EyesController>();
        
        Callable.From(Setup).CallDeferred();
        
        _eyeball = GetNode<Sprite2D>("Eyeball");
        _pupil = GetNode<Sprite2D>("Pupil");

        _controller.TreeExiting += QueueFree;
    }

    private void Setup()
    {
        _followTarget = new Node2D();
        _controller.EyeOwner.AddChild(_followTarget);
        _followTarget.GlobalPosition = GlobalPosition;

        _virtualGlobalPosition = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        DoFollowTarget();
        DoMovePupil();
        UpdateTextureChange(delta);
    }

    public void SwitchTexture(Texture2D texture)
    {
        const float delay = 0.5f;
        _pupil.Hide();
        _eyeball.Texture = texture;
        _textureSwitchTimer = delay;
    }

    private readonly ShakeTween _shakeTween = new ShakeTween()
        .PositionShakeMagnitude(16)
        .InTime(0.05f)
        .OutTime(0.45f);

    public void Shake()
    {
        _shakeTween.Play(_eyeball);
    }

    private void UpdateTextureChange(double delta)
    {
        if (_textureSwitchTimer <= 0)
        {
            return;
        }
        
        _textureSwitchTimer -= delta;
        if (_textureSwitchTimer > 0)
        {
            return;
        }
        
        _pupil.Show();
        _eyeball.Texture = EyeTextures.Default;
    }

    private float _velocity;

    // Tracks the eye's lagged position in global space, independent of the
    // parent's transform. Each frame the parent's transform pulls the eye's
    // GlobalPosition along with it; we overwrite it with this value so the
    // lag behavior survives without reparenting (which would break draw order).
    private Vector2 _virtualGlobalPosition;

    private void DoFollowTarget()
    {
        _eyeball.GlobalRotation = _controller.GlobalRotation;

        const float velocityDecreaseFactor = 0.85f;
        _velocity *= velocityDecreaseFactor;

        var distance = _virtualGlobalPosition.DistanceTo(_followTarget.GlobalPosition);

        const float followSpeedIncrease = 0.1f;
        const float maxVelocity = 1000;

        var increase = Min(maxVelocity, distance * followSpeedIncrease);
        _velocity += increase;

        _virtualGlobalPosition = _virtualGlobalPosition.MoveToward(_followTarget.GlobalPosition, _velocity);
        GlobalPosition = _virtualGlobalPosition;
    }

    private void DoMovePupil()
    {
        Vector2 direction;
        if (_controller.LookTarget.HasValue)
        {
            direction = GlobalPosition.DirectionTo(_controller.LookTarget.Value);
        }
        else
        {
            direction = _controller.GetFallbackLookDirection();
        }

        const float moveSpeed = 0.3f;
        var position = direction * DistanceFromCenter;
        _pupil.Position = _pupil.Position.MoveToward(position, moveSpeed);
    }
    
}

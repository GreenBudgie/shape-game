public partial class Boomerang : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b7okcgfv47m0a");
    private static readonly Texture2D BoomerangParticleTexture =  GD.Load<Texture2D>("uid://6f1okxb4ba2q");
    
    private static readonly AudioStream LaunchSound = GD.Load<AudioStream>("uid://yfgpb5sac6st");
    private static readonly AudioStream FlySound = GD.Load<AudioStream>("uid://c7n4xg4kk54ws");
    
    public static Boomerang Create()
    {
        var node = Scene.Instantiate<Boomerang>();
        return node;
    }

    private float _airSpin;
    private Vector2? _launchDirection;

    protected override void OnReady()
    {
        TrailParticles.Create(this)
            .WithTexture(BoomerangParticleTexture)
            .WithScale(1, 0.2f)
            .Spawn();
        
        AngularVelocity = RandomUtils.RandomSign() * Tau * 2;
        var sound = SoundManager.Instance.PlayPositionalSound(this, LaunchSound);
        sound.RandomizePitchOffset(0.2f);
        
        var initialImpulseComponent = this.GetSingleComponent<InitialImpulseComponent>();
        initialImpulseComponent.Spread = 30;
    }

    private double _timeToActivatePlayerCollision = 0.2f;

    protected override void OnProcess(double delta)
    {
        if (_timeToActivatePlayerCollision <= 0)
        {
            return;
        }
        
        _timeToActivatePlayerCollision -= delta;
        if (_timeToActivatePlayerCollision <= 0)
        {
            this.EnableCollisionLayer(CollisionLayers.PlayerCollider);
            this.EnableCollisionMask(CollisionLayers.PlayerColliderMask);
        }
    }

    private float? _prevRotation;
    private double _timeBetweenFlySounds;
    private const double MinTimeBetweenFlySounds = 0.1;  
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        
        if (_timeBetweenFlySounds > 0)
        {
            _timeBetweenFlySounds -= delta;
            return;
        }

        if (!_prevRotation.HasValue)
        {
            _prevRotation = Rotation;
            return;
        }

        if (_prevRotation > Pi / 2 && Rotation <= Pi / 2)
        {
            var sound = SoundManager.Instance.PlayPositionalSound(this, FlySound);
            sound.RandomizePitchOffset(0.2f);
            _timeBetweenFlySounds = MinTimeBetweenFlySounds;
        }
        
        _prevRotation = Rotation;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        const float airResponse = 2f;
        const float baseLift = 0.02f;
        const float lateralLift = 0.3f;
        const float returnLift = 0.02f;

        _airSpin = Lerp(_airSpin, state.AngularVelocity, 1f - Exp(-airResponse * state.Step));
        var direction = state.LinearVelocity.Normalized();
        var launchDirection = _launchDirection ??= direction;
        var sideFlow = Max(0f, launchDirection.Cross(direction) * Sign(_airSpin));
        var liftCoefficient = baseLift + lateralLift * sideFlow + returnLift * (1f - launchDirection.Dot(direction));
        var angularDeflection = _airSpin * liftCoefficient * state.InverseMass * state.Step;
        state.LinearVelocity = state.LinearVelocity.Rotated(angularDeflection);
    }
}


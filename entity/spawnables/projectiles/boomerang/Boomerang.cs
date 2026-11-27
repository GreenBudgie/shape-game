public partial class Boomerang : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b7okcgfv47m0a");
    
    public static Boomerang Create()
    {
        var node = Scene.Instantiate<Boomerang>();
        return node;
    }

    private float _airSpin;
    private Vector2? _launchDirection;

    protected override void OnReady()
    {
        AngularVelocity = RandomUtils.RandomSign() * Tau * 2;
        
        var initialImpulseComponent = this.GetSingleComponent<InitialImpulseComponent>();
        initialImpulseComponent.Spread = 30;
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


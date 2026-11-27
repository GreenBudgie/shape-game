public partial class Boomerang : BasicRigidBodyProjectile
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://b7okcgfv47m0a");
    
    public static Boomerang Create()
    {
        var node = Scene.Instantiate<Boomerang>();
        return node;
    }

    private float _airSpin;

    protected override void OnReady()
    {
        AngularVelocity = RandomUtils.RandomSign() * Tau * 2;
        
        var initialImpulseComponent = this.GetSingleComponent<InitialImpulseComponent>();
        initialImpulseComponent.Spread = 30;
    }

    // public override void _PhysicsProcess(double delta)
    // {
    //     const float maxRotationSpeed = 1000f;
    //     var vectorToSource = GlobalPosition - _initialPosition;
    //     var angleToSource = LinearVelocity.AngleTo(vectorToSource);
    //     var angleRatio = 1 - angleToSource / Pi; // 0 - boomerang aligned, 1 - just launched
    //     var speed = maxRotationSpeed * angleRatio;
    //     var rotatedVelocity = LinearVelocity.Normalized().Rotated(4 * Pi / 6 * _rotationSign);
    //     ApplyCentralForce(rotatedVelocity * speed);
    //     
    //     DebugDraw.DrawArrow(GlobalPosition, GlobalPosition + rotatedVelocity * speed);
    //     DebugDraw.DrawArrow(GlobalPosition, GlobalPosition + LinearVelocity);
    // }
    
    // public override void _PhysicsProcess(double delta)
    // {
    //     const float returnSpeed = 1f;
    //     const float angleDragSpeed = 1000f;
    //     var vectorToSource = GlobalPosition - _initialPosition;
    //     var rotatedVelocity = LinearVelocity.Normalized().Rotated(4 * Pi / 6 * _rotationSign);
    //     ApplyCentralForce(rotatedVelocity * angleDragSpeed);
    //     ApplyCentralForce(-vectorToSource * returnSpeed);
    // }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        const float airResponse = 3f;
        const float liftCoefficient = 0.13f;

        _airSpin = Lerp(_airSpin, state.AngularVelocity, 1f - Exp(-airResponse * state.Step));
        var angularDeflection = _airSpin * liftCoefficient * state.InverseMass * state.Step;
        state.LinearVelocity = state.LinearVelocity.Rotated(angularDeflection);
    }
}


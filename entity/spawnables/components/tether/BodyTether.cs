public partial class BodyTether : Node2D
{

    private RigidBody2D _source;
    private RigidBody2D _target;
    private Beam _beam;
    private Tween _tween;

    public BodyTether(RigidBody2D source, RigidBody2D target)
    {
        _source = source;
        _target = target;
        
        var targetVector = target.GlobalPosition - source.GlobalPosition;
        
        var joint = new DampedSpringJoint2D
        {
            NodeA = source.GetPath(),
            NodeB = target.GetPath(),
            Length = targetVector.Length(),
            RestLength = targetVector.Length() * 1.2f,
            Stiffness = 10f,
            Damping = 0.5f,
            Rotation = targetVector.Angle() - Pi / 2 - source.GlobalRotation,
        };

        AddChild(joint);

        _beam = Beam.Create();
        _beam.SetFromTo(source.GlobalPosition, target.GlobalPosition);
        _beam.Energy = 0;
        _beam.Progress = 0;
        _beam.Thickness = 20;
        _beam.OutlineThickness = 50;
        _beam.OutlineColor = ColorScheme.Orange;
        _beam.BeamColor = ColorScheme.Orange.Lightened(0.5f);
        _beam.ZIndex -= 1;

        _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        _tween.TweenProperty(_beam, Beam.ProgressProperty, 1f, 0.2f);

        ShapeGame.Instance.AddChild(_beam);
    }

    public override void _ExitTree()
    {
        _tween.Kill();
        var beamTween = _beam.CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        beamTween.TweenProperty(_beam, Beam.ProgressProperty, 0f, 0.2f);
        beamTween.Finished += _beam.QueueFree;
    }

    public override void _Process(double delta)
    {
        if (!IsInstanceValid(_source) || !IsInstanceValid(_target))
        {
            QueueFree();
            return;
        }
        
        _beam.SetFromTo(_source.GlobalPosition, _target.GlobalPosition);
    }
}

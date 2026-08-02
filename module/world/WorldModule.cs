using System.Linq;

public partial class WorldModule : RigidBody2D
{
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://d2oibytyvv11i");
    private static readonly PackedScene ConnectionShapeScene = GD.Load<PackedScene>("uid://do5oc5gogcakt");
    private static readonly PackedScene HexShapeScene = GD.Load<PackedScene>("uid://cqy7bjj6axtb2");

    public Module Module { get; private set; } = null!;

    private Sprite2D _fillSprite = null!;
    
    public static WorldModule Create(Module module)
    {
        var worldModule = Scene.Instantiate<WorldModule>();
        worldModule.Module = module;
        
        var player = Player.FindPlayer();
        if (player != null)
        {
            worldModule.GlobalPosition = player.GlobalPosition;
        }
        else
        {
            worldModule.GlobalPosition = ShapeGame.Center;
        }

        worldModule.Rotation = GD.Randf() * Tau;
        
        return worldModule;
    }

    public override void _Ready()
    {
        _fillSprite = GetNode<Sprite2D>("FillSprite");
        _fillSprite.Texture = Module.Shape.FillTexture;
        _fillSprite.Modulate = ColorScheme.DarkOrange;
        
        var outlineSprite = GetNode<Sprite2D>("OutlineSprite");
        outlineSprite.Texture = Module.Shape.OutlineTexture;
        outlineSprite.Modulate = Module.Color;
        
        var moduleSprite = GetNode<Sprite2D>("ModuleSprite");
        moduleSprite.Texture = Module.Texture;

        var hexPositions = Module.Shape.CenteredPixelHexPositions;
        foreach (var hex in hexPositions)
        {
            var hexShape = HexShapeScene.Instantiate<CollisionPolygon2D>();
            hexShape.Position = hex.Value;
            AddChild(hexShape);
        }

        var positions = hexPositions.Values.ToList();
        for (var i = 0; i < positions.Count; i++)
        {
            for (var j = i + 1; j < positions.Count; j++)
            {
                var connectionShape = ConnectionShapeScene.Instantiate<CollisionShape2D>();
                connectionShape.Position = (positions[i] + positions[j]) / 2;
                connectionShape.Rotation = (positions[j] - positions[i]).Angle();
                AddChild(connectionShape);
            }
        }

        Modulate = Colors.Transparent;
        Scale = new Vector2(1.2f, 1.2f);
        PlayDropAnimation();
    }
    
    private bool _impulsesApplied;
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (_impulsesApplied)
        {
            return;
        }
        
        _impulsesApplied = true;

        var torque = RandomUtils.RandomSignedDeltaRange(20000, 5000);
        ApplyTorqueImpulse(torque);
        
        var impulseDirection = GD.Randf() * Tau;
        var impulseStrength = (float)GD.RandRange(50f, 300f);
        var impulse = Vector2.FromAngle(impulseDirection) * impulseStrength;
        ApplyCentralImpulse(impulse);
    }

    private Tween? _animationTween;

    private void PlayDropAnimation()
    {
        const float duration = 0.2f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _animationTween.FadeIn(this, duration);
        _animationTween.TweenScaleReset(this, duration);
    }

}
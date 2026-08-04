using System.Linq;

public partial class WorldModule : RigidBody2D
{

    private const float AlphaTweenDuration = 0.2f;
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://d2oibytyvv11i");
    private static readonly PackedScene ConnectionShapeScene = GD.Load<PackedScene>("uid://do5oc5gogcakt");
    private static readonly PackedScene HexShapeScene = GD.Load<PackedScene>("uid://cqy7bjj6axtb2");

    public Module Module { get; private set; } = null!;

    private Node2D _spritesNode = null!;
    private Glow _glow = null!;
    private Area2D _playerDetectionArea = null!;

    private bool _isHovered;
    private bool _isSelected;
    private bool _isRemoving;
    
    private Tween? _animationTween;
    private Tween? _alphaTween;
    
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
        
        _spritesNode = GetNode<Node2D>("Sprites");
        
        var fillSprite = _spritesNode.GetNode<Sprite2D>("FillSprite");
        fillSprite.Texture = Module.Shape.FillTexture;
        fillSprite.SelfModulate = ColorScheme.DarkOrange;
        
        var outlineSprite = _spritesNode.GetNode<Sprite2D>("OutlineSprite");
        outlineSprite.Texture = Module.Shape.OutlineTexture;
        outlineSprite.SelfModulate = Module.Color;
        
        var moduleSprite = _spritesNode.GetNode<Sprite2D>("ModuleSprite");
        moduleSprite.Texture = Module.Texture;
        
        _playerDetectionArea = GetNode<Area2D>("PlayerDetectionArea");
        _playerDetectionArea.BodyEntered += OnPlayerHovered;
        _playerDetectionArea.BodyExited += OnPlayerUnhovered;
        
        _glow = Glow.AddGlow(fillSprite)
            .SetColor(Module.Color.AsTransparent())
            .SetRadius(0)
            .SetStrength(1);

        var hexPositions = Module.Shape.CenteredPixelHexPositions;
        foreach (var hex in hexPositions)
        {
            var hexShape = HexShapeScene.Instantiate<CollisionPolygon2D>();
            hexShape.Position = hex.Value;
            AddChild(hexShape);
            _playerDetectionArea.AddChild(hexShape.Duplicate());
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
                _playerDetectionArea.AddChild(connectionShape.Duplicate());
            }
        }

        Modulate = Colors.Transparent;
        Scale = new Vector2(1.2f, 1.2f);
        PlayDropAnimation();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_isRemoving)
        {
            return;
        }
        
        if (InventoryManager.Instance.IsOpen)
        {
            return;
        }
        
        if (!_isSelected)
        {
            return;
        }
        
        if (!@event.IsActionPressed("use"))
        {
            return;
        }
        
        PickUp();
        GetViewport().SetInputAsHandled();
    }

    private void OnPlayerHovered(Node2D _)
    {
        if (_isRemoving)
        {
            return;
        }

        _isHovered = true;
        WorldModuleManager.Instance.ModuleHovered(this);
    }
    
    private void OnPlayerUnhovered(Node2D _)
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isHovered = false;
        WorldModuleManager.Instance.ModuleUnhovered(this);
    }
    
    public void OnSelect()
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isSelected = true;
        
        const float duration = 0.1f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _animationTween.TweenScale(_spritesNode, 1.1f, duration);
        _animationTween.TweenGlowRadius(_glow, 30, duration);
        
        var glowFadeInTweener = _animationTween.TweenGlowFadeIn(_glow, duration);
        if (_alphaTween != null && _alphaTween.IsRunning())
        {
            glowFadeInTweener.SetDelay(AlphaTweenDuration - _alphaTween.GetTotalElapsedTime());
        }
    }
    
    public void OnDeselect()
    {
        if (_isRemoving)
        {
            return;
        }
        
        _isSelected = false;
        
        const float duration = 0.1f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _animationTween.TweenScaleReset(_spritesNode, duration);
        _animationTween.TweenGlowRadius(_glow, 0, duration);
        _animationTween.TweenGlowFadeOut(_glow, duration);
    }
    
    private bool _isFirstPhysicsFrame = true;
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (!_isFirstPhysicsFrame)
        {
            return;
        }
        
        _isFirstPhysicsFrame = false;

        var torque = RandomUtils.RandomSignedDeltaRange(20000, 5000);
        ApplyTorqueImpulse(torque);
        
        var impulseDirection = GD.Randf() * Tau;
        var impulseStrength = (float)GD.RandRange(50f, 300f);
        var impulse = Vector2.FromAngle(impulseDirection) * impulseStrength;
        ApplyCentralImpulse(impulse);
    }

    public bool IsHovered()
    {
        return _isHovered && !_isRemoving;
    }

    private void PickUp()
    {
        InventoryManager.Instance.AddModule(Module);
        Remove();
    }

    private void PlayDropAnimation()
    {
        if (_isRemoving)
        {
            return;
        }
        
        const float duration = 0.2f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _animationTween.TweenScaleReset(_spritesNode, duration);
        
        _alphaTween?.Kill();
        _alphaTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _alphaTween.FadeIn(this, AlphaTweenDuration);
    }
    
    private void Remove()
    {
        if (_isRemoving)
        {
            return;
        }

        _isRemoving = true;

        WorldModuleManager.Instance.OnModuleRemoved(this);

        const float duration = 0.2f;
        
        _animationTween?.Kill();
        _animationTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _animationTween.TweenScale(_spritesNode, 1.2f, duration);
        _animationTween.TweenGlowRadius(_glow, 0, duration / 4);
        _animationTween.TweenGlowFadeOut(_glow, duration / 4);
        
        _alphaTween?.Kill();
        _alphaTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        _alphaTween.FadeOut(this, AlphaTweenDuration);

        _alphaTween.Finished += QueueFree;
    }

}
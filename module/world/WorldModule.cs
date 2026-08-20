using System.Linq;

public partial class WorldModule : RigidBody2D
{
    private const float DefaultDamp = 2.0f;
    private const float PinnedDamp = 6.0f;

    private const float AlphaTweenDuration = 0.2f;
    
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://d2oibytyvv11i");
    private static readonly PackedScene ConnectionShapeScene = GD.Load<PackedScene>("uid://do5oc5gogcakt");
    private static readonly PackedScene HexShapeScene = GD.Load<PackedScene>("uid://cqy7bjj6axtb2");

    public ModuleType ModuleType { get; private set; } = null!;

    private Node2D _spritesNode = null!;
    private Glow _glow = null!;
    private Area2D _playerDetectionArea = null!;
    private HBoxContainer _priceContainer = null!;
    private Label _priceLabel = null!;

    private bool _isHovered;
    private bool _isSelected;
    private bool _isRemoving;
    private bool _isInShop;
    private Vector2? _pinPoint;
    
    private Tween? _animationTween;
    private Tween? _alphaTween;
    
    public static WorldModule Create(ModuleType moduleType)
    {
        var worldModule = Scene.Instantiate<WorldModule>();
        worldModule.ModuleType = moduleType;
        
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
        fillSprite.Texture = ModuleType.Shape.FillTexture;
        fillSprite.SelfModulate = ColorScheme.DarkOrange;
        
        var outlineSprite = _spritesNode.GetNode<Sprite2D>("OutlineSprite");
        outlineSprite.Texture = ModuleType.Shape.OutlineTexture;
        outlineSprite.SelfModulate = ModuleType.Color;
        
        var moduleSprite = _spritesNode.GetNode<Sprite2D>("ModuleSprite");
        moduleSprite.Texture = ModuleType.Texture;
        
        _playerDetectionArea = GetNode<Area2D>("PlayerDetectionArea");
        _playerDetectionArea.BodyEntered += OnPlayerHovered;
        _playerDetectionArea.BodyExited += OnPlayerUnhovered;
        
        _glow = Glow.AddGlow(fillSprite)
            .SetColor(ModuleType.Color.AsTransparent())
            .SetRadius(0)
            .SetStrength(1);

        var hexPositions = ModuleType.Shape.CenteredPixelHexPositions;
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
        
        const float priceGap = 50f;
        var lowestY = _playerDetectionArea.GetCollisionRects().MaxBy(rect => rect.End.Y).End.Y;
        
        _priceContainer = GetNode<HBoxContainer>("PriceContainer"); 
        _priceContainer.GlobalPosition = _priceContainer.GlobalPosition with { Y = lowestY + priceGap };
        _priceContainer.Visible = false;
        
        _priceLabel = GetNode<Label>("PriceContainer/PriceLabel");
        _priceLabel.Text = ModuleType.Price.ToString();

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
        
        if (ScreenManager.Instance.IsAnyScreenOpen())
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

    public override void _PhysicsProcess(double delta)
    {
        RetainPosition();
        RetainRotation();
    }
    
    private void RetainPosition()
    {
        // TODO code is stolen from Barrier
        if (!_pinPoint.HasValue)
        {
            return;
        }
        
        const float force = 0.1f;
        var direction = _pinPoint.Value - GlobalPosition;
        var speed = GlobalPosition.DistanceSquaredTo(_pinPoint.Value);
        ApplyCentralForce(speed * force * direction);
    }

    private void RetainRotation()
    {
        if (!_pinPoint.HasValue)
        {
            return;
        }
        
        // TODO code is stolen from Barrier
        var rotationDegrees = RotationDegrees;
        if (IsZeroApprox(rotationDegrees))
        {
            return;
        }

        const float torqueByDegree = 10000f;
        var direction = rotationDegrees < 0 ? 1 : -1;
        var torque = Abs(rotationDegrees) * torqueByDegree;
        ApplyTorque(torque * direction);
    }

    private Tween? _priceTween;

    public void SetInShop()
    {
        _isInShop = true;
        _pinPoint = GlobalPosition;

        LinearDamp = PinnedDamp;
        AngularDamp = PinnedDamp;

        _priceContainer.Modulate = _priceContainer.Modulate.AsTransparent();
        
        _priceTween?.Kill();
        _priceTween = CreateTween().SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
        _priceTween.FadeIn(_priceContainer, 0.2f);
        
        _priceContainer.Visible = true;
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
        if (_isInShop)
        {
            var canBuy = CrystalManager.Instance.Crystals >= ModuleType.Price;
            if (!canBuy)
            {
                return;
            }
            
            CrystalManager.Instance.Crystals -= ModuleType.Price;
        }
        
        InventoryManager.Instance.AddModule(ModuleType);
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
    
    public void Remove()
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
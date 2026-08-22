public partial class ModuleEffect : Node2D
{
    
    private ModuleType _moduleType = null!;
    private string? _text;
    private Color? _textColor;

    private ModuleDisplay _display = null!; 
    
    public static ModuleEffect Create(ModuleType module)
    {
        var node = new ModuleEffect();
        node._moduleType = module;
        return node;
    }

    public ModuleEffect WithText(string text)
    {
        _text = text;
        return this;
    }
    
    public ModuleEffect WithTextColor(Color textColor)
    {
        _textColor = textColor;
        return this;
    }

    public void Spawn()
    {
        ShapeGame.Instance.AddChild(this);
    }

    public override void _Ready()
    {
        GlobalPosition = GetRandomDisplayPosition();
        
        _display = ModuleDisplay.Create(_moduleType);
        AddChild(_display);
        
        if (_text != null)
        {
            ShowText(_text);
        }
        
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        const float maxAlpha = 0.75f;
        const float maxSize = 0.75f;
        const float maxRotationDelta = 30;
        RotationDegrees = RandomUtils.DeltaRange(0, maxRotationDelta / 2);
        
        const float initDuration = 0.15f;
        const float duration = 1f;
        
        const float minRadius = 10;
        const float maxRadius = 40;
        
        var positionTween = CreateTween()
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        
        var finalPosition = RandomUtils.RandomPointInRadii(minRadius, maxRadius);
        var finalRotation = RandomUtils.DeltaRange(RotationDegrees, maxRotationDelta / 2);
        positionTween.TweenPosition(this, finalPosition, duration + initDuration)
            .AsRelative();
        positionTween.Parallel().TweenRotationDegrees(this, finalRotation, duration + initDuration)
            .SetEase(Tween.EaseType.Out);
        
        Modulate = Colors.Transparent;
        Scale = Vector2.Zero;
        
        var modulateTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
        
        modulateTween.TweenAlpha(this, maxAlpha, initDuration).SetEase(Tween.EaseType.Out);
        modulateTween.Parallel().TweenScale(this, maxSize, initDuration).SetEase(Tween.EaseType.Out);
        
        modulateTween.FadeOut(this, duration).SetEase(Tween.EaseType.In);
        const float minSize = 0.5f;
        modulateTween.Parallel().TweenScale(this, new Vector2(minSize, minSize), duration)
            .SetEase(Tween.EaseType.In);
        
        modulateTween.Finished += QueueFree;
    }

    private void ShowText(string text)
    {
        var smallerShapeSize = _moduleType.Shape.PixelSize * 0.33f;
        var positionOffset = new Vector2(
            RandomUtils.DeltaRange(0, smallerShapeSize.X),
            RandomUtils.DeltaRange(0, smallerShapeSize.Y)
        );
        var labelPosition = GlobalPosition + positionOffset;
        
        var label = PopupLabel.Create(labelPosition, text);
        if (_textColor.HasValue)
        {
            label.SetColor(_textColor.Value);
        }
    }

    private Vector2 GetRandomDisplayPosition()
    {
        var player = Player.FindPlayer();
        if (player == null)
        {
            // Usually this should not happen, but just in case spawn at center
            return ShapeGame.Center;
        }
        
        var shapeSize = _moduleType.Shape.PixelSize;
        var distanceFromShapeCenter = Max(shapeSize.X, shapeSize.Y) / 2f; 
        var distanceFromPlayer = Max(Player.MaxVisibleSize.X, Player.MaxVisibleSize.Y) / 2f;
        
        const float maxDeviation = 50f;
        var minLength = distanceFromShapeCenter + distanceFromPlayer;
        var maxLength = minLength + maxDeviation;
        var length = RandomUtils.Range(minLength, maxLength);
        var vector = RandomUtils.RandomNormalizedVector() * length;

        return player.GlobalPosition + vector;
    }
    
}
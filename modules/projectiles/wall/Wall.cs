public partial class Wall : StaticBody2D, IProjectile<Wall>
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cgm0yfj1g1own");

    private static readonly StringName ProgressMaskParam = "progress";
    private static readonly NodePath ProgressMaskShaderParam = ShaderParameter(ProgressMaskParam);

    private static readonly Vector2 LeftCornerPosition = new(-225, 0);
    private static readonly Vector2 RightCornerPosition = new(225, 0);

    private Sprite2D _sprite = null!;
    private ShaderMaterial _spriteMaterial = null!;

    public Wall Node => this;

    public static Wall Create()
    {
        var node = Scene.Instantiate<Wall>();
        return node;
    }

    public void Prepare(ShotContext context)
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _spriteMaterial = (ShaderMaterial)_sprite.Material;
        _spriteMaterial.SetShaderParameter(ProgressMaskParam, 0f);

        const float yOffset = 250f;
        GlobalPosition += new Vector2(0, -yOffset);
    }

    private Glow _glow = null!;

    public override void _Ready()
    {
        _glow = Glow.AddGlow(_sprite, addChild: false);
        _glow.Sprite.Scale = new Vector2(0, 1);
        _glow.SetColor(ColorScheme.LightBlueGreen)
            .SetStrength(1)
            .SetRadius(30);
        
        AddChild(_glow);

        var player = Player.FindPlayer();
        if (player != null)
        {
            PlaySpawnBeamEffectForPlayer(player);
        }

        PlayAppearEffect();
    }

    private Beam? _leftBeam;
    private Beam? _rightBeam;

    public override void _Process(double delta)
    {
        if (_leftBeam == null || _rightBeam == null)
        {
            return;
        }

        var player = Player.FindPlayer();
        if (player == null)
        {
            return;
        }

        if (IsInstanceValid(_leftBeam))
        {
            _leftBeam.SetFrom(player.GetGlobalNosePosition());
        }

        if (IsInstanceValid(_rightBeam))
        {
            _rightBeam.SetFrom(player.GetGlobalNosePosition());
        }
    }

    private const float EffectStartupDuration = 0.15f;
    private const float EffectDuration = 0.3f;
    private const float EffectEndDuration = 0.15f;

    private void PlayAppearEffect()
    {
        var tween = CreateTween();
        tween.TweenProperty(_spriteMaterial, ProgressMaskShaderParam, 1f, EffectDuration)
            .SetDelay(EffectStartupDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);

        _glow.Sprite.Scale = new Vector2(0, 1);
        tween.Parallel().TweenProperty(_glow.Sprite, ScaleProperty, Vector2.One, EffectDuration)
            .SetDelay(EffectStartupDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
    }

    private void PlaySpawnBeamEffectForPlayer(Player player)
    {
        _leftBeam = CreateAndAnimateBeam(player, ToGlobal(LeftCornerPosition));
        _rightBeam = CreateAndAnimateBeam(player, ToGlobal(RightCornerPosition));
    }

    private Beam CreateAndAnimateBeam(Player player, Vector2 toPosition)
    {
        var beam = Beam.Create()
            .SetFromTo(player.GetGlobalNosePosition(), GlobalPosition)
            .SetEnergy(0)
            .SetProgress(0)
            .SetOutlineThickness(100)
            .SetOutlineColor(ColorScheme.LightBlueGreen)
            .SetBeamColor(ColorScheme.LightBlueGreen.Lightened(0.5f));

        var positionTween = beam.CreateTween();
        positionTween.TweenMethod(
                Callable.From<Vector2>(a => beam.SetTo(a)),
                GlobalPosition,
                toPosition,
                EffectDuration
            )
            .SetDelay(EffectStartupDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);

        var tween = beam.CreateTween();
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 1, EffectStartupDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 0, EffectEndDuration)
            .SetDelay(EffectDuration)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Finished += () => beam.QueueFree();
        ShapeGame.Instance.AddChild(beam);

        return beam;
    }
}
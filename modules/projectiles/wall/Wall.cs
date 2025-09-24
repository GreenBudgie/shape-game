public partial class Wall : StaticBody2D, IProjectile<Wall>
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cgm0yfj1g1own");

    private static readonly Vector2 LeftCornerPosition = new(-225, 0);
    private static readonly Vector2 RightCornerPosition = new(225, 0);

    public Wall Node => this;

    public static Wall Create()
    {
        var node = Scene.Instantiate<Wall>();
        return node;
    }

    public void Prepare(ShotContext context)
    {
        const float yOffset = 250f;
        GlobalPosition += new Vector2(0, -yOffset);
    }

    public override void _Ready()
    {
        var player = Player.FindPlayer();
        if (player != null)
        {
            PlaySpawnEffect(player);
        }
    }

    private void PlaySpawnEffect(Player player)
    {
        var beam = Beam.Create()
            .SetFromTo(player.GetGlobalNosePosition(), ToGlobal(LeftCornerPosition))
            .SetEnergy(0)
            .SetProgress(0)
            .SetOutlineThickness(100)
            .SetOutlineColor(ColorScheme.LightGreen)
            .SetBeamColor(ColorScheme.LightGreen.Lightened(0.5f));

        var positionTween = beam.CreateTween();
        positionTween.TweenMethod(
            Callable.From<Vector2>(a => beam.SetTo(a)),
            ToGlobal(LeftCornerPosition),
            ToGlobal(RightCornerPosition),
            1
        );

        var tween = beam.CreateTween();
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 1, 0.5)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quint);
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 0, 0.5)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Finished += () => beam.QueueFree();
        ShapeGame.Instance.AddChild(beam);
    }
}
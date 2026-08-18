using System.Collections.Immutable;
using System.Linq;

public partial class LevelBoundary : StaticBody2D
{

    private const float DefaultGlowRadius = 96;
    private const float DefaultGlowStrength = 2;
    private const float TweenDuration = 0.25f;
    private const float GlowTweenDuration = 0.5f;

    public ImmutableList<RectGlow> Glows { get; private set; } = null!;

    private Tween? _glowColorTween;
    private Tween? _glowTween;
    
    public override void _Ready()
    {
        Glows = GetChildren().OfType<RectGlow>().ToImmutableList();
    }
    
    public void DisableGlow()
    {
        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
        foreach (var glow in Glows)
        {
            _glowTween.TweenGlowStrength(glow, 0, TweenDuration);
            _glowTween.TweenGlowRadius(glow, 0, TweenDuration);
        }
    }

    public void EnableGlowWithColor(Color color)
    {
        _glowTween?.Kill();
        _glowColorTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
        _glowColorTween = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
        foreach (var glow in Glows)
        {
            _glowTween.TweenGlowStrength(glow, DefaultGlowStrength, TweenDuration);
            _glowTween.TweenGlowRadius(glow, DefaultGlowRadius, TweenDuration);
            _glowColorTween.TweenGlowColor(glow, color, GlowTweenDuration);
        }
    }
    
}

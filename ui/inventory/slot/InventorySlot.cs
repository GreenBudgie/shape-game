using System;
using Modules;

public partial class InventorySlot : TextureButton
{

    private const float GlowStartDistance = 600f;
    private const float GlowStopDistance = 100f;
    private const float GlowStartDistanceSq = GlowStartDistance * GlowStartDistance;
    private const float GlowMinStrength = 0.8f;
    private const float GlowMaxStrength = 1.2f;
    private const float GlowMaxRadius = 60f;

    private const float GlowHoverStrength = 1.5f;
    private const float GlowHoverRadius = 80f;
    private const float GlowHoverTweenDuration = 0.25f;
    private const float GlowUnhoverTweenDuration = 0.5f;

    [Export] private Color _glowColor;

    private Glow _glow = null!;
    private Tween? _glowTween;
    private Tween? _transformTween;
    private bool _hovered;

    public override void _Ready()
    {
        _glow = Glow.AddGlow(this)
            .SetColor(_glowColor)
            .TurnOff();

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;
    }

    public override void _Process(double delta)
    {
        if (_hovered)
        {
            return;
        }

        var isTweenRunning = _glowTween?.IsValid() == true && _glowTween?.IsRunning() == true;

        var centeredPosition = GlobalPosition + PivotOffset;
        var distanceToMouseSq = GetGlobalMousePosition().DistanceSquaredTo(centeredPosition);
        if (distanceToMouseSq > GlowStartDistanceSq)
        {
            if (!isTweenRunning)
            {
                _glow.TurnOff();
            }

            return;
        }

        var distanceToMouse = Sqrt(distanceToMouseSq);
        var distanceRatio = 1 - (distanceToMouse - GlowStopDistance) / (GlowStartDistance - GlowStopDistance);
        var glowStrength = Min(Lerp(distanceRatio, GlowMinStrength, GlowMaxStrength), GlowMaxStrength);
        var glowRadius = Min(distanceRatio * GlowMaxRadius, GlowMaxRadius);

        if (!isTweenRunning)
        {
            _glow.Strength = glowStrength;
            _glow.Radius = glowRadius;
            return;
        }

        if (glowStrength >= _glow.Strength)
        {
            _glow.Strength = glowStrength;
            _glowTween?.Kill();
        }
        
        if (glowRadius >= _glow.Radius)
        {
            _glow.Radius = glowRadius;
            _glowTween?.Kill();
        }
    }

    private void OnMouseEnter()
    {
        _hovered = true;

        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);

        _glowTween.TweenProperty(
            @object: _glow,
            property: Glow.PropertyName.Strength.ToString(),
            finalVal: GlowHoverStrength,
            duration: GlowHoverTweenDuration
        );
        _glowTween.Parallel().TweenProperty(
            @object: _glow,
            property: Glow.PropertyName.Radius.ToString(),
            finalVal: GlowHoverRadius,
            duration: GlowHoverTweenDuration
        );
        
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        
        _transformTween.TweenProperty(
            @object: this,
            property: Node2D.PropertyName.Scale.ToString(),
            finalVal: new Vector2(1.1f, 1.1f),
            duration: GlowHoverTweenDuration
        );
        
    }

    private void OnMouseExit()
    {
        _hovered = false;

        _glowTween?.Kill();
        _glowTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        
        _glowTween.TweenProperty(
            @object: _glow,
            property: Glow.PropertyName.Strength.ToString(),
            finalVal: 0,
            duration: GlowUnhoverTweenDuration
        );
        _glowTween.Parallel().TweenProperty(
            @object: _glow,
            property: Glow.PropertyName.Radius.ToString(),
            finalVal: 0,
            duration: GlowUnhoverTweenDuration
        );
        
        _transformTween?.Kill();
        _transformTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        
        _transformTween.TweenProperty(
            @object: this,
            property: Node2D.PropertyName.Scale.ToString(),
            finalVal: Vector2.One,
            duration: GlowHoverTweenDuration
        );
        
    }

    public Module InsertModule(Module module)
    {
        if (module == null)
        {
            RemoveModule();
        }

        var previousModule = RemoveModule();
        AddChild(module);
        return previousModule;
    }

    public Module GetModule()
    {
        return GetChildOrNull<Module>(0);
    }

    public Module RemoveModule()
    {
        var module = GetModule();
        if (module == null)
        {
            return null;
        }

        RemoveChild(module);
        return module;
    }

    public bool HasModule()
    {
        return GetModule() != null;
    }

}
/// <summary>
/// Controls a glow shader material applied directly to a CanvasItem.
/// </summary>
public partial class Glow : RefCounted
{
    public static readonly NodePath RadiusProperty = "Radius";
    public static readonly NodePath StrengthProperty = "Strength";

    private static readonly StringName GlowColorName = "glow_color";
    private static readonly StringName GlowRadiusName = "glow_radius";
    private static readonly StringName GlowStrengthName = "glow_strength";

    private static readonly Shader GlowShader = GD.Load<Shader>("uid://b5llrlesa8ji2");

    public CanvasItem Target { get; private set; }= null!;
    
    private ShaderMaterial _shaderMaterial = null!;

    private bool _isPulsing;
    private float _pulseRadiusDelta;
    private float _pulseStrengthDelta;
    private float _pulsesPerSecond = 1.0f;

    private float? _baseStrength;
    private float? _baseRadius;
    private Color? _cachedColor;

    public Color Color
    {
        get => GetColor();
        set => SetColor(value);
    }

    public float Radius
    {
        get => GetBaseRadius();
        set => SetRadius(value);
    }

    public float Strength
    {
        get => GetBaseStrength();
        set => SetStrength(value);
    }

    private void Init(CanvasItem target)
    {
        Target = target;

        if (target.Material is ShaderMaterial existing && existing.Shader == GlowShader)
        {
            _shaderMaterial = existing;
        }
        else
        {
            _shaderMaterial = new ShaderMaterial { Shader = GlowShader };
            target.Material = _shaderMaterial;
        }

        _baseStrength = GetBaseStrength();
        _baseRadius = GetBaseRadius();

        target.GetTree().ProcessFrame += HandlePulsing;
        target.TreeExiting += Remove;
    }

    /// <summary>
    /// Must be called every frame from the node's _Process to handle pulsing.
    /// </summary>
    public void Process()
    {
        HandlePulsing();
    }

    private void HandlePulsing()
    {
        if (!_isPulsing || (_pulseRadiusDelta == 0 && _pulseStrengthDelta == 0))
            return;

        var t = Time.GetTicksMsec() / 1000f;
        var pulse = 0.5f + 0.5f * Sin(t * Tau * _pulsesPerSecond);
        UpdateRadius(GetBaseRadius() + _pulseRadiusDelta * pulse);
        UpdateStrength(GetBaseStrength() + _pulseStrengthDelta * pulse);
    }

    public Glow SetColor(Color color)
    {
        if (_cachedColor.HasValue && _cachedColor.Value == color)
            return this;
        _cachedColor = color;
        _shaderMaterial.SetShaderParameter(GlowColorName, color);
        return this;
    }

    public Glow SetRadius(float radius)
    {
        var r = Max(radius, 0);
        if (IsEqualApprox(GetBaseRadius(), r))
            return this;
        _baseRadius = r;
        UpdateRadius(r);
        return this;
    }

    private void UpdateRadius(float radius) =>
        _shaderMaterial.SetShaderParameter(GlowRadiusName, radius);

    public Glow SetStrength(float strength)
    {
        var s = Max(strength, 0);
        if (IsEqualApprox(GetBaseStrength(), s))
            return this;
        _baseStrength = s;
        UpdateStrength(s);
        return this;
    }

    private void UpdateStrength(float strength) =>
        _shaderMaterial.SetShaderParameter(GlowStrengthName, strength);

    public Glow TurnOff()
    {
        SetRadius(0);
        SetStrength(0);
        return this;
    }

    /// <summary>
    /// Removes the glow shader from the target, restoring original material.
    /// </summary>
    public void Remove()
    {
        if (Target.IsInsideTree())
        {
            Target.GetTree().ProcessFrame -= HandlePulsing;
        }

        Target.TreeExiting -= Remove;
        Target.Material = null;
    }

    public Color GetColor()
    {
        if (_cachedColor.HasValue) return _cachedColor.Value;
        _cachedColor = (Color)_shaderMaterial.GetShaderParameter(GlowColorName);
        return _cachedColor.Value;
    }

    public float GetBaseRadius()
    {
        if (_baseRadius.HasValue) return _baseRadius.Value;
        _baseRadius = (float)_shaderMaterial.GetShaderParameter(GlowRadiusName);
        return _baseRadius.Value;
    }

    public float GetBaseStrength()
    {
        if (_baseStrength.HasValue) return _baseStrength.Value;
        _baseStrength = (float)_shaderMaterial.GetShaderParameter(GlowStrengthName);
        return _baseStrength.Value;
    }

    public Glow EnablePulsing()
    {
        _baseStrength = GetBaseStrength();
        _baseRadius = GetBaseRadius();
        _isPulsing = true;
        return this;
    }

    public Glow DisablePulsing()
    {
        _isPulsing = false;
        SetStrength(GetBaseStrength());
        SetRadius(GetBaseRadius());
        return this;
    }

    public Glow SetPulsesPerSecond(float pulses)
    {
        _pulsesPerSecond = pulses;
        return this;
    }

    public Glow SetPulseStrengthDelta(float delta)
    {
        _pulseStrengthDelta = delta;
        return this;
    }

    public Glow SetPulseRadiusDelta(float delta)
    {
        _pulseRadiusDelta = delta;
        return this;
    }

    public bool IsPulsing() => _isPulsing;
    public float GetPulseStrengthDelta() => _pulseStrengthDelta;
    public float GetPulseRadiusDelta() => _pulseRadiusDelta;
    public float GetPulsesPerSecond() => _pulsesPerSecond;

    /// <summary>
    /// Creates a Glow controller for any CanvasItem (Sprite2D, TextureRect, etc.)
    /// </summary>
    public static Glow AddGlow(CanvasItem target)
    {
        var glow = new Glow();
        glow.Init(target);
        return glow;
    }
}
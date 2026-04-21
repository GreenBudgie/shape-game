/// <summary>
/// A component for adding a glow effect to a Sprite2D or TextureRect using a shader.
/// The Glow node is a Sprite2D child that renders behind its parent using a glow shader.
/// </summary>
public partial class Glow : CanvasGroup
{
    public static readonly NodePath RadiusProperty = PropertyName.Radius.ToString();
    public static readonly NodePath StrengthProperty = PropertyName.Strength.ToString();

    private static readonly StringName GlowColorName = "glow_color";
    private static readonly StringName GlowRadiusName = "glow_radius";
    private static readonly StringName GlowStrengthName = "glow_strength";

    private static readonly PackedScene GlowScene = GD.Load<PackedScene>("uid://c3qvnso7dnntg");

    private ShaderMaterial _shaderMaterial;

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

    public Glow()
    {
        _shaderMaterial = (ShaderMaterial)Material;
    }

    public override void _Ready()
    {
        _baseStrength = GetBaseStrength();
        _baseRadius = GetBaseRadius();
    }

    public override void _Process(double delta)
    {
        HandlePulsing();
    }

    private void HandlePulsing()
    {
        if (!_isPulsing || (_pulseRadiusDelta == 0 && _pulseStrengthDelta == 0))
        {
            return;
        }

        var t = Time.GetTicksMsec() / 1000f;
        var pulse = 0.5f + 0.5f * Sin(t * Tau * _pulsesPerSecond);
        UpdateRadius(GetBaseRadius() + _pulseRadiusDelta * pulse);
        UpdateStrength(GetBaseStrength() + _pulseStrengthDelta * pulse);
    }

    /// <summary>
    /// Sets the glow color of the effect.
    /// </summary>
    public Glow SetColor(Color color)
    {
        if (_cachedColor.HasValue && _cachedColor.Value == color)
        {
            return this;
        }

        _cachedColor = color;
        _shaderMaterial.SetShaderParameter(GlowColorName, color);
        return this;
    }

    /// <summary>
    /// Sets the blur radius of the glow effect.
    /// </summary>
    public Glow SetRadius(float radius)
    {
        var positiveRadius = Max(radius, 0);
        if (IsEqualApprox(GetBaseRadius(), positiveRadius))
        {
            return this;
        }

        _baseRadius = positiveRadius;
        UpdateRadius(positiveRadius);
        return this;
    }

    private void UpdateRadius(float radius)
    {
        const float safeMargin = 4;
        FitMargin = radius + safeMargin;
        _shaderMaterial.SetShaderParameter(GlowRadiusName, radius);
    }

    /// <summary>
    /// Sets the overall strength (opacity) of the glow.
    /// </summary>
    public Glow SetStrength(float strength)
    {
        var positiveStrength = Max(strength, 0);
        if (IsEqualApprox(GetBaseStrength(), positiveStrength))
        {
            return this;
        }

        _baseStrength = positiveStrength;
        UpdateStrength(positiveStrength);
        return this;
    }

    private void UpdateStrength(float strength)
    {
        _shaderMaterial.SetShaderParameter(GlowStrengthName, strength);
    }

    /// <summary>
    /// Sets the strength and radius of the glow to zero, turning it off.
    /// </summary>
    public Glow TurnOff()
    {
        SetRadius(0);
        SetStrength(0);
        return this;
    }

    /// <summary>
    /// Gets the current color used for the glow.
    /// </summary>
    public Color GetColor()
    {
        if (_cachedColor.HasValue)
        {
            return _cachedColor.Value;
        }

        _cachedColor = (Color)_shaderMaterial.GetShaderParameter(GlowColorName);
        return _cachedColor.Value;
    }

    /// <summary>
    /// Gets the current blur radius of the glow effect.
    /// </summary>
    public float GetBaseRadius()
    {
        if (_baseRadius.HasValue)
        {
            return _baseRadius.Value;
        }

        _baseRadius = (float)_shaderMaterial.GetShaderParameter(GlowRadiusName);
        return _baseRadius.Value;
    }

    /// <summary>
    /// Gets the current strength (opacity multiplier) of the glow.
    /// </summary>
    public float GetBaseStrength()
    {
        if (_baseStrength.HasValue)
        {
            return _baseStrength.Value;
        }

        _baseStrength = (float)_shaderMaterial.GetShaderParameter(GlowStrengthName);
        return _baseStrength.Value;
    }

    /// <summary>
    /// Enables pulsing effect.
    /// </summary>
    public Glow EnablePulsing()
    {
        _baseStrength = GetBaseStrength();
        _baseRadius = GetBaseRadius();
        _isPulsing = true;
        return this;
    }

    /// <summary>
    /// Disables the pulsing effect.
    /// </summary>
    public Glow DisablePulsing()
    {
        _isPulsing = false;
        SetStrength(GetBaseStrength());
        SetRadius(GetBaseRadius());
        return this;
    }

    /// <summary>
    /// Sets how many pulses per second should occur.
    /// </summary>
    public Glow SetPulsesPerSecond(float pulses)
    {
        _pulsesPerSecond = pulses;
        return this;
    }

    /// <summary>
    /// Sets how much the strength should vary during pulsing.
    /// </summary>
    public Glow SetPulseStrengthDelta(float delta)
    {
        _pulseStrengthDelta = delta;
        return this;
    }

    /// <summary>
    /// Sets how much the radius should vary during pulsing.
    /// </summary>
    public Glow SetPulseRadiusDelta(float delta)
    {
        _pulseRadiusDelta = delta;
        return this;
    }

    public bool IsPulsing() => _isPulsing;
    public float GetPulseStrengthDelta() => _pulseStrengthDelta;
    public float GetPulseRadiusDelta() => _pulseRadiusDelta;
    public float GetPulsesPerSecond() => _pulsesPerSecond;

}
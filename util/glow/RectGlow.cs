/// <summary>
/// A glow effect for a rectangular area, rendered on a ColorRect using a shader.
/// Emulates the outer glow of a solid white rectangle, so no texture is needed.
/// The shader is fed the node's size via the rect_size uniform.
/// </summary>
public partial class RectGlow : ColorRect, IGlow
{
    private static readonly StringName GlowColorName = "glow_color";
    private static readonly StringName GlowRadiusName = "glow_radius";
    private static readonly StringName GlowStrengthName = "glow_strength";
    private static readonly StringName RectSizeName = "rect_size";

    private ShaderMaterial _shaderMaterial;

    private float? _baseStrength;
    private float? _baseRadius;
    private Color? _cachedColor;

    // Hides ColorRect.Color (the fill color, unused here) so that Color refers to the glow color.
    public new Color Color
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

    public RectGlow()
    {
        _shaderMaterial = (ShaderMaterial)Material;
    }

    public override void _Ready()
    {
        _shaderMaterial.SetShaderParameter(RectSizeName, Size);
        _baseStrength = GetBaseStrength();
        _baseRadius = GetBaseRadius();
    }

    /// <summary>
    /// Sets the glow color of the effect.
    /// </summary>
    public new RectGlow SetColor(Color color)
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
    public RectGlow SetRadius(float radius)
    {
        var positiveRadius = Max(radius, 0);
        if (IsEqualApprox(GetBaseRadius(), positiveRadius))
        {
            return this;
        }

        _baseRadius = positiveRadius;
        _shaderMaterial.SetShaderParameter(GlowRadiusName, positiveRadius);
        return this;
    }

    /// <summary>
    /// Sets the overall strength (opacity) of the glow.
    /// </summary>
    public RectGlow SetStrength(float strength)
    {
        var positiveStrength = Max(strength, 0);
        if (IsEqualApprox(GetBaseStrength(), positiveStrength))
        {
            return this;
        }

        _baseStrength = positiveStrength;
        _shaderMaterial.SetShaderParameter(GlowStrengthName, positiveStrength);
        return this;
    }

    /// <summary>
    /// Sets the strength and radius of the glow to zero, turning it off.
    /// </summary>
    public RectGlow TurnOff()
    {
        SetRadius(0);
        SetStrength(0);
        return this;
    }

    void IGlow.TurnOff()
    {
        TurnOff();
    }

    /// <summary>
    /// Gets the current color used for the glow.
    /// </summary>
    public new Color GetColor()
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
}

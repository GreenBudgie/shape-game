/// <summary>
/// A component for adding a glow effect to a Sprite2D using a shader.
/// </summary>
public partial class Glow : SubViewportContainer
{
    private static readonly StringName GlowColor = "glow_color";
    private static readonly StringName GlowRadius = "glow_radius";
    private static readonly StringName GlowStrength = "glow_strength";
    private static readonly StringName CullOccluded = "cull_occluded";

    private static readonly PackedScene GlowScene = GD.Load<PackedScene>("uid://c3qvnso7dnntg");

    private ShaderMaterial _shaderMaterial = null!;

    private bool _isPulsing;
    private float _pulseRadiusDelta = 0.0f;
    private float _pulseStrengthDelta = 0.0f;
    private float _pulsesPerSecond = 1.0f;

    private float _baseStrength;
    private float _baseRadius;

    public override void _Ready()
    {
        _shaderMaterial = (ShaderMaterial)Material;
        _baseStrength = GetStrength();
        _baseRadius = GetRadius();
    }

    public override void _Process(double delta)
    {
        HandlePulsing();
    }

    private void HandlePulsing()
    {
        if (!_isPulsing)
        {
            return;
        }

        var t = Time.GetTicksMsec() / 1000f;
        var pulse = 0.5f + 0.5f * Sin(t * Tau * _pulsesPerSecond);
        UpdateRadius(_baseRadius + _pulseRadiusDelta * pulse);
        UpdateStrength(_baseStrength + _pulseStrengthDelta * pulse);
    }

    /// <summary>
    /// Sets the glow color of the effect.
    /// </summary>
    /// <param name="color">The color to apply to the glow (alpha included).</param>
    /// <returns>The current Glow instance for chaining.</returns>
    public Glow SetColor(Color color)
    {
        _shaderMaterial.SetShaderParameter(GlowColor, color);
        return this;
    }

    /// <summary>
    /// Sets the blur radius of the glow effect.
    /// </summary>
    /// <param name="radius">The blur radius in pixels.</param>
    /// <returns>The current Glow instance for chaining.</returns>
    public Glow SetRadius(float radius)
    {
        _baseRadius = Max(radius, 0);
        UpdateRadius(_baseRadius);
        return this;
    }
    
    private void UpdateRadius(float radius)
    {
        _shaderMaterial.SetShaderParameter(GlowRadius, radius);
    }
    
    /// <summary>
    /// Sets the overall strength (opacity) of the glow.
    /// </summary>
    /// <param name="strength">The strength multiplier (e.g., 1.0 = normal, 0.5 = dim).</param>
    /// <returns>The current Glow instance for chaining.</returns>
    public Glow SetStrength(float strength)
    {
        _baseStrength = Max(strength, 0);
        UpdateStrength(strength);
        return this;
    }
    
    private void UpdateStrength(float strength)
    {
        _shaderMaterial.SetShaderParameter(GlowStrength, strength);
    }

    public Glow SetCullOccluded(bool cullOccluded)
    {
        _shaderMaterial.SetShaderParameter(CullOccluded, cullOccluded);
        return this;
    }

    /// <summary>
    /// Gets the current color used for the glow.
    /// </summary>
    /// <returns>The glow color.</returns>
    public Color GetColor()
    {
        return (Color)_shaderMaterial.GetShaderParameter(GlowColor);
    }

    /// <summary>
    /// Gets the current blur radius of the glow effect.
    /// </summary>
    /// <returns>The blur radius in pixels.</returns>
    public float GetRadius()
    {
        return (float)_shaderMaterial.GetShaderParameter(GlowRadius);
    }

    /// <summary>
    /// Gets the current strength (opacity multiplier) of the glow.
    /// </summary>
    /// <returns>The glow strength.</returns>
    public float GetStrength()
    {
        return (float)_shaderMaterial.GetShaderParameter(GlowStrength);
    }

    public bool IsCullOccluded()
    {
        return (bool)_shaderMaterial.GetShaderParameter(CullOccluded);
    }

    /// <summary>
    /// Enables pulsing effect based on the configured parameters.
    /// </summary>
    public Glow EnablePulsing()
    {
        _baseStrength = GetStrength();
        _baseRadius = GetRadius();
        _isPulsing = true;
        return this;
    }

    /// <summary>
    /// Disables the pulsing effect.
    /// </summary>
    public Glow DisablePulsing()
    {
        _isPulsing = false;
        SetStrength(_baseStrength);
        SetRadius(_baseRadius);
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

    /// <summary>
    /// Gets whether pulsing is currently enabled.
    /// </summary>
    public bool IsPulsing() => _isPulsing;

    /// <summary>
    /// Gets the current pulse strength delta.
    /// </summary>
    public float GetPulseStrengthDelta() => _pulseStrengthDelta;

    /// <summary>
    /// Gets the current pulse radius delta.
    /// </summary>
    public float GetPulseRadiusDelta() => _pulseRadiusDelta;

    /// <summary>
    /// Gets the configured pulses per second.
    /// </summary>
    public float GetPulsesPerSecond() => _pulsesPerSecond;

    /// <summary>
    /// Adds a Glow instance as a child to the given Sprite2D, with the same texture.
    /// </summary>
    /// <param name="sprite">The sprite to attach the glow to.</param>
    /// <returns>The instantiated Glow node.</returns>
    public static Glow AddGlow(Sprite2D sprite)
    {
        var glow = GlowScene.Instantiate<Glow>();
        var subViewport = glow.GetNode<SubViewport>("SubViewport");
        var glowSprite = glow.GetNode<Sprite2D>("SubViewport/Sprite");
        
        sprite.AddChild(glow);
        
        var texture = sprite.Texture;
        subViewport.Size = (Vector2I)(texture.GetSize() * 2);
        glowSprite.Texture = texture;
        glowSprite.Position = subViewport.Size / 2;
        glow.ZIndex = sprite.ZIndex - 1;
        glow.Size = subViewport.Size;
        glow.Position = -subViewport.Size / 2;

        return glow;
    }
}
using System.Diagnostics;

/// <summary>
/// A component for adding a glow effect to a Sprite2D or TextureRect using a shader.
/// </summary>
public partial class Glow : SubViewportContainer
{

    public static readonly NodePath RadiusProperty = PropertyName.Radius.ToString();
    public static readonly NodePath StrengthProperty = PropertyName.Strength.ToString();
    
    private static readonly StringName GlowColorName = "glow_color";
    private static readonly StringName GlowRadiusName = "glow_radius";
    private static readonly StringName GlowStrengthName = "glow_strength";
    private static readonly StringName CullOccludedName = "cull_occluded";

    private static readonly PackedScene GlowScene = GD.Load<PackedScene>("uid://c3qvnso7dnntg");

    public Sprite2D Sprite { get; private set; } = null!; 
    
    private ShaderMaterial _shaderMaterial = null!;

    private bool _isPulsing;
    private float _pulseRadiusDelta = 0.0f;
    private float _pulseStrengthDelta = 0.0f;
    private float _pulsesPerSecond = 1.0f;

    private float? _baseStrength;
    private float? _baseRadius;
    private Color? _cachedColor;
    private bool? _cachedCullOccluded;

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
    
    public bool CullOccluded
    {
        get => IsCullOccluded();
        set => SetCullOccluded(value);
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

        var t = Time.GetTicksMsec() / 1000f; // TODO fps
        var pulse = 0.5f + 0.5f * Sin(t * Tau * _pulsesPerSecond);
        UpdateRadius(GetBaseRadius() + _pulseRadiusDelta * pulse);
        UpdateStrength(GetBaseStrength() + _pulseStrengthDelta * pulse);
    }

    /// <summary>
    /// Sets the glow color of the effect.
    /// </summary>
    /// <param name="color">The color to apply to the glow (alpha included).</param>
    /// <returns>The current Glow instance for chaining.</returns>
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
    /// <param name="radius">The blur radius in pixels.</param>
    /// <returns>The current Glow instance for chaining.</returns>
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
        _shaderMaterial.SetShaderParameter(GlowRadiusName, radius);
    }
    
    /// <summary>
    /// Sets the overall strength (opacity) of the glow.
    /// </summary>
    /// <param name="strength">The strength multiplier (e.g., 1.0 = normal, 0.5 = dim).</param>
    /// <returns>The current Glow instance for chaining.</returns>
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

    public Glow SetCullOccluded(bool cullOccluded)
    {
        if (_cachedCullOccluded == cullOccluded)
        {
            return this;
        }

        _cachedCullOccluded = cullOccluded;
        _shaderMaterial.SetShaderParameter(CullOccludedName, cullOccluded);
        return this;
    }

    /// <summary>
    /// Sets the strength and radius of the glow to zero, turning it "off".
    /// </summary>
    /// <returns>The current Glow instance for chaining.</returns>
    public Glow TurnOff()
    {
        SetRadius(0);
        SetStrength(0);
        return this;
    }

    /// <summary>
    /// Gets the current color used for the glow.
    /// </summary>
    /// <returns>The glow color.</returns>
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
    /// <returns>The blur radius in pixels.</returns>
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
    /// <returns>The glow strength.</returns>
    public float GetBaseStrength()
    {
        if (_baseStrength.HasValue)
        {
            return _baseStrength.Value;
        }

        _baseStrength = (float)_shaderMaterial.GetShaderParameter(GlowStrengthName);
        return _baseStrength.Value;
    }

    public bool IsCullOccluded()
    {
        if (_cachedCullOccluded.HasValue)
        {
            return _cachedCullOccluded.Value;
        }

        _cachedCullOccluded = (bool)_shaderMaterial.GetShaderParameter(CullOccludedName);
        return _cachedCullOccluded.Value;
    }

    /// <summary>
    /// Enables pulsing effect based on the configured parameters.
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
    /// <param name="addChild">Whether to automatically add glow as a child to this node. True by default</param>
    /// <returns>The instantiated Glow node.</returns>
    public static Glow AddGlow(Sprite2D sprite, bool addChild = true)
    {
        return AttachGlow(sprite, sprite.Texture, Vector2.Zero, addChild);
    }
    
    /// <summary>
    /// Adds a Glow instance as a child to the given TextureRect, with the same texture.
    ///
    /// Image in this TextureRect should be centered.
    /// </summary>
    /// <param name="textureRect">The texture rect to attach the glow to.</param>
    /// <param name="addChild">Whether to automatically add glow as a child to this node. True by default</param>
    /// <returns>The instantiated Glow node.</returns>
    public static Glow AddGlow(TextureRect textureRect, bool addChild = true)
    {
        return AttachGlow(textureRect, textureRect.Texture, textureRect.Size / 2, addChild);
    }
    
    /// <summary>
    /// Adds a Glow instance as a child to the given TextureButton, with the texture of normal button state.
    ///
    /// Image in this TextureButton should be centered.
    /// </summary>
    /// <param name="textureButton">The texture button to attach the glow to.</param>
    /// <param name="addChild">Whether to automatically add glow as a child to this node. True by default</param>
    /// <returns>The instantiated Glow node.</returns>
    public static Glow AddGlow(TextureButton textureButton, bool addChild = true)
    {
        return AttachGlow(textureButton, textureButton.TextureNormal, textureButton.Size / 2, addChild);
    }
    
    /// <summary>
    /// Adds a Glow instance as a child to the given TextureProgressBar, with an "over" texture.
    ///
    /// Image in this TextureProgressBar should be centered.
    /// </summary>
    /// <param name="textureProgressBar">The texture progress bar to attach the glow to.</param>
    /// <param name="addChild">Whether to automatically add glow as a child to this node. True by default</param>
    /// <returns>The instantiated Glow node.</returns>
    public static Glow AddGlow(TextureProgressBar textureProgressBar, bool addChild = true)
    {
        return AttachGlow(textureProgressBar, textureProgressBar.TextureOver, textureProgressBar.Size / 2, addChild);
    }

    private static Glow AttachGlow(CanvasItem node, Texture2D texture, Vector2 centerShift, bool addChild)
    {
        var glow = GlowScene.Instantiate<Glow>();
        var subViewport = glow.GetNode<SubViewport>("SubViewport");
        var glowSprite = glow.GetNode<Sprite2D>("SubViewport/Sprite");
        glow.Sprite = glowSprite;

        if (addChild)
        {
            node.AddChild(glow);
        }

        const int viewportSizeMultiplier = 4;
        
        subViewport.Size = (Vector2I)(texture.GetSize() * viewportSizeMultiplier);
        glowSprite.Texture = texture;
        glowSprite.Position = subViewport.Size / 2;
        glow.ZIndex = node.ZIndex - 1;
        glow.Size = subViewport.Size;
        glow.Position = -subViewport.Size / 2 + centerShift;

        return glow;
    }
}
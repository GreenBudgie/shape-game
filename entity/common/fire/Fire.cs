/// <summary>
/// A single flame drawn by the fire shader. The flame is centered in the rect and rises up from its center.
/// </summary>
public partial class Fire : ColorRect
{

    private static readonly Shader FireShader = GD.Load<Shader>("uid://btbn8wfjijlmq");

    private static readonly StringName IntensityParam = "intensity";
    private static readonly StringName IgnitionParam = "ignition";
    private static readonly StringName CoreColorParam = "core_color";
    private static readonly StringName MidColorParam = "mid_color";
    private static readonly StringName OuterColorParam = "outer_color";
    private static readonly StringName GlowColorParam = "glow_color";
    private static readonly StringName GlowStrengthParam = "glow_strength";
    private static readonly StringName SpeedParam = "speed";
    private static readonly StringName FlameHeightParam = "flame_height";
    private static readonly StringName TurbulenceParam = "turbulence";
    private static readonly StringName DetailParam = "detail";
    private static readonly StringName EdgeSoftnessParam = "edge_softness";
    private static readonly StringName NoiseOffsetParam = "noise_offset";
    private static readonly StringName LeanParam = "lean";
    private static readonly StringName RectSizeParam = "rect_size";

    public static readonly NodePath IntensityShaderParam = ShaderParameter(IntensityParam);
    public static readonly NodePath IgnitionShaderParam = ShaderParameter(IgnitionParam);

    /// <summary>
    /// Maximum length of the lean vector. Larger values bend the flame too much to look like fire.
    /// </summary>
    public const float MaxLean = 0.8f;

    /// <summary>
    /// Outer color of the default palette of the fire shader. <see cref="SetFlameColor"/> with this color
    /// gives exactly the default palette.
    /// </summary>
    public static readonly Color DefaultColor = new(1f, 0.25f, 0.04f);

    // Relations between the layers of the default palette in HSV: outer (1, 0.25, 0.04) has hue 13.1°
    // and saturation 0.96, mid (1, 0.7, 0.2) has hue 37.5° and saturation 0.8,
    // glow (1, 0.35, 0.08) has hue 17.6° and saturation 0.92. All of them have value 1.
    private const float MidHueShift = 24.375f / 360f;
    private const float MidSaturationFactor = 0.8f / 0.96f;
    private const float GlowHueShift = 4.484f / 360f;
    private const float GlowSaturationFactor = 0.92f / 0.96f;

    public ShaderMaterial ShaderMaterial { get; }

    private Vector2 _noiseOffset;

    public Fire()
    {
        ShaderMaterial = new ShaderMaterial
        {
            Shader = FireShader
        };
        Material = ShaderMaterial;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public static Fire Create()
    {
        return new Fire();
    }

    /// <summary>
    /// Sets the size of the flame quad. The flame body takes about a half of it, the tongues rise above.
    /// </summary>
    public Fire SetFlameSize(float size)
    {
        Size = new Vector2(size, size);
        PivotOffset = Size / 2;
        ShaderMaterial.SetShaderParameter(RectSizeParam, Size);
        return this;
    }

    /// <summary>
    /// Places the center of the flame quad at the given position, in parent coordinates.
    /// </summary>
    public Fire SetCenter(Vector2 position)
    {
        Position = position - Size / 2;
        return this;
    }

    /// <summary>
    /// Sets the whole palette from a single color, which becomes the outer flame color.
    /// Mid and glow colors keep the same hue and saturation relations to it as in the default palette,
    /// so the default look is kept for any color. The core stays almost white.
    /// See <see cref="DefaultColor"/>.
    /// </summary>
    public Fire SetFlameColor(Color color)
    {
        SetOuterColor(color);
        SetMidColor(ShiftColor(color, MidHueShift, MidSaturationFactor));
        SetGlowColor(ShiftColor(color, GlowHueShift, GlowSaturationFactor));
        return this;
    }

    private static Color ShiftColor(Color color, float hueShift, float saturationFactor)
    {
        return Color.FromHsv(PosMod(color.H + hueShift, 1f), Clamp(color.S * saturationFactor, 0, 1), color.V, color.A);
    }

    /// <summary>
    /// How hot the fire burns. Lower = dim flames without a white core, higher = bigger core and brighter glow.
    ///
    /// <p>Default: 1.0</p>
    /// </summary>
    public Fire SetIntensity(float intensity)
    {
        ShaderMaterial.SetShaderParameter(IntensityParam, intensity);
        return this;
    }

    /// <summary>
    /// Lifecycle of the flame. 0 = no fire, 1 = fully burning.
    ///
    /// <p>Default: 1.0</p>
    /// </summary>
    public Fire SetIgnition(float ignition)
    {
        ShaderMaterial.SetShaderParameter(IgnitionParam, Clamp(ignition, 0, 1));
        return this;
    }

    public Fire SetCoreColor(Color color)
    {
        ShaderMaterial.SetShaderParameter(CoreColorParam, color);
        return this;
    }

    public Fire SetMidColor(Color color)
    {
        ShaderMaterial.SetShaderParameter(MidColorParam, color);
        return this;
    }

    public Fire SetOuterColor(Color color)
    {
        ShaderMaterial.SetShaderParameter(OuterColorParam, color);
        return this;
    }

    public Fire SetGlowColor(Color color)
    {
        ShaderMaterial.SetShaderParameter(GlowColorParam, color);
        return this;
    }

    /// <summary>
    /// <p>Default: 0.5</p>
    /// </summary>
    public Fire SetGlowStrength(float strength)
    {
        ShaderMaterial.SetShaderParameter(GlowStrengthParam, strength);
        return this;
    }

    /// <summary>
    /// Animation speed of the flame.
    ///
    /// <p>Default: 1.5</p>
    /// </summary>
    public Fire SetSpeed(float speed)
    {
        ShaderMaterial.SetShaderParameter(SpeedParam, speed);
        return this;
    }

    /// <summary>
    /// How far the tongues stretch upward.
    ///
    /// <p>Default: 0.7</p>
    /// </summary>
    public Fire SetFlameHeight(float height)
    {
        ShaderMaterial.SetShaderParameter(FlameHeightParam, height);
        return this;
    }

    /// <summary>
    /// How strongly the flame is torn into tongues and swayed.
    ///
    /// <p>Default: 0.7</p>
    /// </summary>
    public Fire SetTurbulence(float turbulence)
    {
        ShaderMaterial.SetShaderParameter(TurbulenceParam, turbulence);
        return this;
    }

    /// <summary>
    /// Noise scale. Higher = more, thinner tongues.
    ///
    /// <p>Default: 3.0</p>
    /// </summary>
    public Fire SetDetail(float detail)
    {
        ShaderMaterial.SetShaderParameter(DetailParam, detail);
        return this;
    }

    /// <summary>
    /// <p>Default: 0.15</p>
    /// </summary>
    public Fire SetEdgeSoftness(float softness)
    {
        ShaderMaterial.SetShaderParameter(EdgeSoftnessParam, softness);
        return this;
    }

    /// <summary>
    /// Sets the shift of the noise pattern, in flame units (half of the flame size = 1).
    /// A random offset makes the flame look different from other flames.
    /// </summary>
    public Fire SetNoiseOffset(Vector2 offset)
    {
        _noiseOffset = offset;
        ShaderMaterial.SetShaderParameter(NoiseOffsetParam, offset);
        return this;
    }

    /// <summary>
    /// Shifts the noise pattern by the given distance, in flame units.
    /// Shifting by the movement of the flame makes the fire evolve and trail behind.
    /// </summary>
    public Fire AddNoiseOffset(Vector2 delta)
    {
        return SetNoiseOffset(_noiseOffset + delta);
    }

    /// <summary>
    /// Bends the flame: the shift of the flame top, in flame units. Clamped to <see cref="MaxLean"/>.
    /// X leans sideways, positive Y squashes the flame, negative Y stretches it upward.
    /// </summary>
    public Fire SetLean(Vector2 lean)
    {
        ShaderMaterial.SetShaderParameter(LeanParam, lean.LimitLength(MaxLean));
        return this;
    }

    /// <summary>
    /// Smoothly ignites the flame from zero.
    /// </summary>
    public Tween Ignite(float duration)
    {
        SetIgnition(0);
        var tween = CreateTween();
        tween.TweenProperty(ShaderMaterial, IgnitionShaderParam, 1f, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        return tween;
    }

    /// <summary>
    /// Smoothly extinguishes the flame from its current state.
    /// </summary>
    public Tween Extinguish(float duration)
    {
        var tween = CreateTween();
        tween.TweenProperty(ShaderMaterial, IgnitionShaderParam, 0f, duration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
        return tween;
    }

}

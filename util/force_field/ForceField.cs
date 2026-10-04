/// <summary>
/// A circular force field with a water-like surface, drawn by the force field shader.
/// The field fills the circle inscribed into the rect, so the rect is always kept square.
/// </summary>
public partial class ForceField : ColorRect
{

    private static readonly Shader ForceFieldShader = GD.Load<Shader>("uid://dqe2wsnoftl3i");

    private static readonly StringName FieldColorParam = "field_color";
    private static readonly StringName IntensityParam = "intensity";
    private static readonly StringName ActivationParam = "activation";
    private static readonly StringName FillStrengthParam = "fill_strength";
    private static readonly StringName RimStrengthParam = "rim_strength";
    private static readonly StringName RimWidthParam = "rim_width";
    private static readonly StringName RimWhitenessParam = "rim_whiteness";
    private static readonly StringName EdgeSoftnessParam = "edge_softness";
    private static readonly StringName RippleModeParam = "ripple_mode";
    private static readonly StringName RippleCountParam = "ripple_count";
    private static readonly StringName RippleSpeedParam = "ripple_speed";
    private static readonly StringName RippleStrengthParam = "ripple_strength";
    private static readonly StringName WobbleParam = "wobble";
    private static readonly StringName DetailParam = "detail";
    private static readonly StringName NoiseOffsetParam = "noise_offset";

    public static readonly NodePath FieldColorShaderParam = ShaderParameter(FieldColorParam);
    public static readonly NodePath IntensityShaderParam = ShaderParameter(IntensityParam);
    public static readonly NodePath ActivationShaderParam = ShaderParameter(ActivationParam);

    /// <summary>
    /// Random noise offsets of fields are picked in this range, so that fields look different from each other.
    /// </summary>
    private const float NoiseOffsetRange = 100f;

    public ShaderMaterial ShaderMaterial { get; }

    /// <summary>
    /// Radius of the fully open field, in local pixels.
    /// </summary>
    public float Radius { get; private set; }

    public ForceField()
    {
        ShaderMaterial = new ShaderMaterial
        {
            Shader = ForceFieldShader
        };
        Material = ShaderMaterial;
        MouseFilter = MouseFilterEnum.Ignore;

        SetNoiseOffset(new Vector2(GD.Randf(), GD.Randf()) * NoiseOffsetRange);
    }

    public static ForceField Create()
    {
        return new ForceField();
    }

    /// <summary>
    /// Sets the radius of the fully open field, in local pixels.
    /// The rect becomes a square with the field inscribed into it.
    /// </summary>
    public ForceField SetRadius(float radius)
    {
        Radius = Max(radius, 0);
        Size = new Vector2(Radius * 2, Radius * 2);
        PivotOffset = Size / 2;
        return this;
    }

    /// <summary>
    /// Places the center of the field at the given position, in parent coordinates.
    /// </summary>
    public ForceField SetCenter(Vector2 position)
    {
        Position = position - Size / 2;
        return this;
    }

    /// <summary>
    /// Color of the field. The rim and the brightest crests shift towards white on top of it.
    ///
    /// <p>Default: (0.22, 0.63, 0.93)</p>
    /// </summary>
    public ForceField SetFieldColor(Color color)
    {
        ShaderMaterial.SetShaderParameter(FieldColorParam, color);
        return this;
    }

    /// <summary>
    /// Overall brightness.
    ///
    /// <p>Default: 1.0</p>
    /// </summary>
    public ForceField SetIntensity(float intensity)
    {
        ShaderMaterial.SetShaderParameter(IntensityParam, intensity);
        return this;
    }

    /// <summary>
    /// Current radius of the field, as a part of <see cref="Radius"/>. 0 = no field, 1 = fully open.
    /// See <see cref="Open"/> and <see cref="Close"/> to animate it.
    ///
    /// <p>Default: 1.0</p>
    /// </summary>
    public ForceField SetActivation(float activation)
    {
        ShaderMaterial.SetShaderParameter(ActivationParam, Clamp(activation, 0, 1));
        return this;
    }

    /// <summary>
    /// Brightness of the area inside the field.
    ///
    /// <p>Default: 0.22</p>
    /// </summary>
    public ForceField SetFillStrength(float strength)
    {
        ShaderMaterial.SetShaderParameter(FillStrengthParam, strength);
        return this;
    }

    /// <summary>
    /// Brightness of the border of the field.
    ///
    /// <p>Default: 1.1</p>
    /// </summary>
    public ForceField SetRimStrength(float strength)
    {
        ShaderMaterial.SetShaderParameter(RimStrengthParam, strength);
        return this;
    }

    /// <summary>
    /// Thickness of the border, as a part of the radius.
    ///
    /// <p>Default: 0.13</p>
    /// </summary>
    public ForceField SetRimWidth(float width)
    {
        ShaderMaterial.SetShaderParameter(RimWidthParam, width);
        return this;
    }

    /// <summary>
    /// How much the border is whitened. 0 keeps the field color everywhere.
    ///
    /// <p>Default: 0.45</p>
    /// </summary>
    public ForceField SetRimWhiteness(float whiteness)
    {
        ShaderMaterial.SetShaderParameter(RimWhitenessParam, whiteness);
        return this;
    }

    /// <summary>
    /// Softness of the outer edge of the inner area, as a part of the radius.
    ///
    /// <p>Default: 0.13</p>
    /// </summary>
    public ForceField SetEdgeSoftness(float softness)
    {
        ShaderMaterial.SetShaderParameter(EdgeSoftnessParam, softness);
        return this;
    }

    /// <summary>
    /// How the ripples move. See <see cref="ForceFieldRippleMode"/>.
    ///
    /// <p>Default: <see cref="ForceFieldRippleMode.Waves"/></p>
    /// </summary>
    public ForceField SetRippleMode(ForceFieldRippleMode mode)
    {
        ShaderMaterial.SetShaderParameter(RippleModeParam, (int)mode);
        return this;
    }

    /// <summary>
    /// Number of waves fitting into the radius.
    ///
    /// <p>Default: 5.0</p>
    /// </summary>
    public ForceField SetRippleCount(float count)
    {
        ShaderMaterial.SetShaderParameter(RippleCountParam, count);
        return this;
    }

    /// <summary>
    /// How fast the waves run and the surface changes.
    ///
    /// <p>Default: 0.5</p>
    /// </summary>
    public ForceField SetRippleSpeed(float speed)
    {
        ShaderMaterial.SetShaderParameter(RippleSpeedParam, speed);
        return this;
    }

    /// <summary>
    /// Contrast of the waves.
    ///
    /// <p>Default: 0.5</p>
    /// </summary>
    public ForceField SetRippleStrength(float strength)
    {
        ShaderMaterial.SetShaderParameter(RippleStrengthParam, strength);
        return this;
    }

    /// <summary>
    /// How strongly the noise bends the waves. 0 gives straight overlapping waves, higher looks like water.
    ///
    /// <p>Default: 0.8</p>
    /// </summary>
    public ForceField SetWobble(float wobble)
    {
        ShaderMaterial.SetShaderParameter(WobbleParam, wobble);
        return this;
    }

    /// <summary>
    /// Scale of the surface noise. Higher = finer bends.
    ///
    /// <p>Default: 2.2</p>
    /// </summary>
    public ForceField SetDetail(float detail)
    {
        ShaderMaterial.SetShaderParameter(DetailParam, detail);
        return this;
    }

    /// <summary>
    /// Shift of the noise pattern. Every field gets a random one on creation, so fields look different
    /// from each other. Set the same value to make several fields look the same.
    /// </summary>
    public ForceField SetNoiseOffset(Vector2 offset)
    {
        ShaderMaterial.SetShaderParameter(NoiseOffsetParam, offset);
        return this;
    }

    /// <summary>
    /// Smoothly opens the field from zero radius.
    /// </summary>
    public Tween Open(float duration)
    {
        SetActivation(0);
        var tween = CreateTween();
        tween.TweenProperty(ShaderMaterial, ActivationShaderParam, 1f, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        return tween;
    }

    /// <summary>
    /// Smoothly closes the field from its current state.
    /// </summary>
    public Tween Close(float duration)
    {
        var tween = CreateTween();
        tween.TweenProperty(ShaderMaterial, ActivationShaderParam, 0f, duration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.In);
        return tween;
    }

}

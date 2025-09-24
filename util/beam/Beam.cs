public partial class Beam : ColorRect
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://g21wng36wpva");

    // StringName constants for shader parameters
    private static readonly StringName BeamsParam = "beams";
    private static readonly StringName EnergyParam = "energy";
    private static readonly StringName RoughnessParam = "roughness";
    private static readonly StringName FrequencyParam = "frequency";
    private static readonly StringName SpeedParam = "speed";
    private static readonly StringName ThicknessParam = "thickness";
    private static readonly StringName OutlineThicknessParam = "outline_thickness";
    private static readonly StringName BeamDifferenceParam = "beam_difference";
    private static readonly StringName GlowParam = "glow";
    private static readonly StringName OutlineGlowParam = "outline_glow";
    private static readonly StringName ColorParam = "color";
    private static readonly StringName OutlineColorParam = "outline_color";
    private static readonly StringName ProgressParam = "progress";
    private static readonly StringName BeamLengthParam = "beam_length";
    private static readonly StringName FadeDistanceParam = "fade_distance";

    public static readonly NodePath BeamsShaderParam = ShaderParameter(BeamsParam);
    public static readonly NodePath EnergyShaderParam = ShaderParameter(EnergyParam);
    public static readonly NodePath RoughnessShaderParam = ShaderParameter(RoughnessParam);
    public static readonly NodePath FrequencyShaderParam = ShaderParameter(FrequencyParam);
    public static readonly NodePath SpeedShaderParam = ShaderParameter(SpeedParam);
    public static readonly NodePath ThicknessShaderParam = ShaderParameter(ThicknessParam);
    public static readonly NodePath OutlineThicknessShaderParam = ShaderParameter(OutlineThicknessParam);
    public static readonly NodePath BeamDifferenceShaderParam = ShaderParameter(BeamDifferenceParam);
    public static readonly NodePath GlowShaderParam = ShaderParameter(GlowParam);
    public static readonly NodePath OutlineGlowShaderParam = ShaderParameter(OutlineGlowParam);
    public static readonly NodePath ColorShaderParam = ShaderParameter(ColorParam);
    public static readonly NodePath OutlineColorShaderParam = ShaderParameter(OutlineColorParam);
    public static readonly NodePath ProgressShaderParam = ShaderParameter(ProgressParam);
    public static readonly NodePath BeamLengthShaderParam = ShaderParameter(BeamLengthParam);
    public static readonly NodePath FadeDistanceShaderParam = ShaderParameter(FadeDistanceParam);

    private const float BaseSizeMultiplier = 1.5f;
    private static readonly Vector2 BaseSize = ShapeGame.WindowSize * BaseSizeMultiplier; // (5760, 3240)
    
    private ShaderMaterial _shaderMaterial;

    public Beam()
    {
        _shaderMaterial = (ShaderMaterial)Material;
    }
    
    public static Beam Create()
    {
        var beam = Scene.Instantiate<Beam>();
        beam.SetAnchorsPreset(LayoutPreset.TopLeft);
        beam.Size = BaseSize;
        return beam;
    }

    /// <summary>
    /// Sets the start and end point for the bean in global coordinates
    /// </summary>
    /// <param name="from">From position, in global coords</param>
    /// <param name="to">To position, in global coords</param>
    public Beam SetFromTo(Vector2 from, Vector2 to)
    {
        Position = from - new Vector2(0, BaseSize.Y / 2);
        Rotation = from.AngleToPoint(to);
        var length = from.DistanceTo(to) / Size.X;
        SetLength(length);

        const float fadeDistanceToLengthFactor = 0.05f;
        SetFadeDistance(length * fadeDistanceToLengthFactor);
        return this;
    }

    /// <summary>
    /// Sets the number of beams
    ///
    /// <p>Default: 2</p>
    /// </summary>
    public Beam SetBeamCount(int count)
    {
        _shaderMaterial.SetShaderParameter(BeamsParam, count);
        return this;
    }

    /// <summary>
    /// Sets the energy - how much the beams will travel up and down.
    /// If zero, the beam will not move.
    /// 
    /// <p>Default: 3.0</p>
    /// </summary>
    public Beam SetEnergy(float energy)
    {
        _shaderMaterial.SetShaderParameter(EnergyParam, energy);
        return this;
    }

    /// <summary>
    /// Sets the roughness - how "noisy" the beam is. Higher values produce more chaotic beams.
    /// If zero, the beam will not move.
    /// 
    /// <p>Default: 3</p>
    /// </summary>
    public Beam SetRoughness(int roughness)
    {
        _shaderMaterial.SetShaderParameter(RoughnessParam, roughness);
        return this;
    }

    /// <summary>
    /// Sets the frequency - amount of "ripples" in the beams.
    /// If zero, the beam will not move.
    /// 
    /// <p>Default: 3</p>
    /// </summary>
    public Beam SetFrequency(int frequency)
    {
        _shaderMaterial.SetShaderParameter(FrequencyParam, frequency);
        return this;
    }

    /// <summary>
    /// Sets the animation speed.
    /// If zero, the beam will be frozen, but still can be distorted by other parameters.
    /// 
    /// <p>Default: 1</p>
    /// </summary>
    public Beam SetSpeed(float speed)
    {
        _shaderMaterial.SetShaderParameter(SpeedParam, speed);
        return this;
    }

    /// <summary>
    /// Sets the progress, or the amount of beam visible on screen. Can be used to slowly show the beam.
    /// If zero, the beam will be hidden.
    /// 
    /// <p>Default: 1</p>
    /// </summary>
    public Beam SetProgress(float progress)
    {
        _shaderMaterial.SetShaderParameter(ProgressParam, progress);
        return this;
    }

    /// <summary>
    /// Sets the thickness of the beam (center part of it), in pixels.
    /// If zero, the beam center part will not be visible.
    ///
    /// <p>Default: ~30px (set in UV in shader, equal to 0.0045)</p>
    /// </summary>
    public Beam SetThickness(float thickness)
    {
        var realThickness = thickness / BaseSize.Y / 2;
        _shaderMaterial.SetShaderParameter(ThicknessParam, realThickness);
        return this;
    }

    /// <summary>
    /// Sets the outline thickness of the beam, in pixels. This is an absolute value and is not relative to beam
    /// center part thickness.
    /// Minimum noticeable width is 150px, otherwise it's too dim.
    ///
    /// <p>Default: ~150px (set in UV in shader, equal to 0.023)</p>
    /// </summary>
    public Beam SetOutlineThickness(float outlineThickness)
    {
        var realThickness = outlineThickness / BaseSize.Y / 2;
        _shaderMaterial.SetShaderParameter(OutlineThicknessParam, realThickness);
        return this;
    }

    /// <summary>
    /// The thickness difference between the main beam and the other, if there are more than one beam.
    /// The closer to 1 the smaller the thickness difference.
    ///
    /// <p>Default: 0</p>
    /// </summary>
    public Beam SetBeamDifference(float difference)
    {
        _shaderMaterial.SetShaderParameter(BeamDifferenceParam, difference);
        return this;
    }

    /// <summary>
    /// Sets the color of the beam (its center part). 
    /// </summary>
    public Beam SetBeamColor(Color color)
    {
        _shaderMaterial.SetShaderParameter(ColorParam, color);
        return this;
    }

    /// <summary>
    /// Sets the outline color
    /// </summary>
    public Beam SetOutlineColor(Color color)
    {
        _shaderMaterial.SetShaderParameter(OutlineColorParam, color);
        return this;
    }

    /// <summary>
    /// Sets the beam length relative to its total width, from 0 to 1.
    /// Can be used to animate the beam "shooting" without actually moving the from/to points.
    /// </summary>
    public Beam SetLength(float length)
    {
        _shaderMaterial.SetShaderParameter(BeamLengthParam, length);
        return this;
    }

    public Beam SetFadeDistance(float distance)
    {
        _shaderMaterial.SetShaderParameter(FadeDistanceParam, distance);
        return this;
    }

}
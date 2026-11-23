public partial class Beam : ColorRect
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://g21wng36wpva");

    private static readonly StringName BeamsParam = "beams";
    private static readonly StringName EnergyParam = "energy";
    private static readonly StringName RoughnessParam = "roughness";
    private static readonly StringName FrequencyParam = "frequency";
    private static readonly StringName SpeedParam = "speed";
    private static readonly StringName ThicknessParam = "thickness";
    private static readonly StringName OutlineThicknessParam = "outline_thickness";
    private static readonly StringName BeamDifferenceParam = "beam_difference";
    private static readonly StringName ColorParam = "color";
    private static readonly StringName OutlineColorParam = "outline_color";
    private static readonly StringName ProgressParam = "progress";
    private static readonly StringName BeamLengthParam = "beam_length";
    private static readonly StringName FadeDistanceParam = "fade_distance";

    public static readonly NodePath FromProperty = nameof(From);
    public static readonly NodePath ToProperty = nameof(To);
    public static readonly NodePath BeamCountProperty = nameof(BeamCount);
    public static readonly NodePath EnergyProperty = nameof(Energy);
    public static readonly NodePath RoughnessProperty = nameof(Roughness);
    public static readonly NodePath FrequencyProperty = nameof(Frequency);
    public static readonly NodePath SpeedProperty = nameof(Speed);
    public static readonly NodePath ProgressProperty = nameof(Progress);
    public static readonly NodePath ThicknessProperty = nameof(Thickness);
    public static readonly NodePath OutlineThicknessProperty = nameof(OutlineThickness);
    public static readonly NodePath BeamDifferenceProperty = nameof(BeamDifference);
    public static readonly NodePath BeamColorProperty = nameof(BeamColor);
    public static readonly NodePath OutlineColorProperty = nameof(OutlineColor);
    public static readonly NodePath LengthProperty = nameof(Length);
    public static readonly NodePath FadeDistanceProperty = nameof(FadeDistance);

    private const float BaseSizeMultiplier = 1.5f;
    private static readonly Vector2 BaseSize = ShapeGame.WindowSize * BaseSizeMultiplier;
    private static readonly float PixelsPerThicknessUnit = BaseSize.Y * 2;

    public ShaderMaterial ShaderMaterial => (ShaderMaterial)Material;

    public static Beam Create()
    {
        var beam = Scene.Instantiate<Beam>();
        beam.SetAnchorsPreset(LayoutPreset.TopLeft);
        beam.Size = BaseSize;
        return beam;
    }

    private Vector2 _from;
    private Vector2 _to;

    /// <summary>Start point in global coordinates.</summary>
    public Vector2 From
    {
        get => _from;
        set => SetFromTo(value, _to);
    }

    /// <summary>End point in global coordinates.</summary>
    public Vector2 To
    {
        get => _to;
        set => SetFromTo(_from, value);
    }

    /// <summary>Updates both endpoints and recalculates the beam geometry once.</summary>
    public void SetFromTo(Vector2 from, Vector2 to)
    {
        _from = from;
        _to = to;
        Position = from - new Vector2(0, BaseSize.Y / 2);
        Rotation = from.AngleToPoint(to);
        var length = from.DistanceTo(to) / Size.X;
        Length = length;

        // const float fadeDistanceToLengthFactor = 0.2f;
        // FadeDistance = length * fadeDistanceToLengthFactor;
    }

    /// <summary>Number of beams to draw. Defaults to 2.</summary>
    public int BeamCount
    {
        get => (int)ShaderMaterial.GetShaderParameter(BeamsParam);
        set => ShaderMaterial.SetShaderParameter(BeamsParam, value);
    }

    /// <summary>Amplitude of the beam's vertical motion. Defaults to 3.</summary>
    public float Energy
    {
        get => (float)ShaderMaterial.GetShaderParameter(EnergyParam);
        set => ShaderMaterial.SetShaderParameter(EnergyParam, value);
    }

    /// <summary>Number of noise octaves. Higher values make the beam rougher. Defaults to 3.</summary>
    public int Roughness
    {
        get => (int)ShaderMaterial.GetShaderParameter(RoughnessParam);
        set => ShaderMaterial.SetShaderParameter(RoughnessParam, value);
    }

    /// <summary>Frequency of ripples along the beam. Defaults to 3.</summary>
    public int Frequency
    {
        get => (int)ShaderMaterial.GetShaderParameter(FrequencyParam);
        set => ShaderMaterial.SetShaderParameter(FrequencyParam, value);
    }

    /// <summary>Animation speed. Set to zero to freeze the noise animation. Defaults to 1.</summary>
    public float Speed
    {
        get => (float)ShaderMaterial.GetShaderParameter(SpeedParam);
        set => ShaderMaterial.SetShaderParameter(SpeedParam, value);
    }

    /// <summary>Visibility of the beam, from 0 (hidden) to 1 (fully visible).</summary>
    public float Progress
    {
        get => (float)ShaderMaterial.GetShaderParameter(ProgressParam);
        set => ShaderMaterial.SetShaderParameter(ProgressParam, value);
    }

    /// <summary>Thickness of the beam center in pixels. Defaults to approximately 30 pixels.</summary>
    public float Thickness
    {
        get => (float)ShaderMaterial.GetShaderParameter(ThicknessParam) * PixelsPerThicknessUnit;
        set => ShaderMaterial.SetShaderParameter(ThicknessParam, value / PixelsPerThicknessUnit);
    }

    /// <summary>Outline thickness in pixels, independent of the center thickness. Defaults to approximately 150 pixels.</summary>
    public float OutlineThickness
    {
        get => (float)ShaderMaterial.GetShaderParameter(OutlineThicknessParam) * PixelsPerThicknessUnit;
        set => ShaderMaterial.SetShaderParameter(OutlineThicknessParam, value / PixelsPerThicknessUnit);
    }

    /// <summary>Values closer to 1 make additional beams closer in thickness to the main beam. Defaults to 0.</summary>
    public float BeamDifference
    {
        get => (float)ShaderMaterial.GetShaderParameter(BeamDifferenceParam);
        set => ShaderMaterial.SetShaderParameter(BeamDifferenceParam, value);
    }

    public Color BeamColor
    {
        get => (Color)ShaderMaterial.GetShaderParameter(ColorParam);
        set => ShaderMaterial.SetShaderParameter(ColorParam, value);
    }

    public Color OutlineColor
    {
        get => (Color)ShaderMaterial.GetShaderParameter(OutlineColorParam);
        set => ShaderMaterial.SetShaderParameter(OutlineColorParam, value);
    }

    /// <summary>Length of the beam as a fraction of the rect width.</summary>
    public float Length
    {
        get => (float)ShaderMaterial.GetShaderParameter(BeamLengthParam);
        set => ShaderMaterial.SetShaderParameter(BeamLengthParam, value);
    }

    /// <summary>Fade distance at each end as a fraction of the rect width.</summary>
    public float FadeDistance
    {
        get => (float)ShaderMaterial.GetShaderParameter(FadeDistanceParam);
        set => ShaderMaterial.SetShaderParameter(FadeDistanceParam, value);
    }

}

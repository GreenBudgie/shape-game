public partial class Beam : ColorRect
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://g21wng36wpva");

    private static readonly Vector2 BaseSize = ShapeGame.WindowSize * 1.5f;
    
    private ShaderMaterial _shaderMaterial;

    public Beam()
    {
        _shaderMaterial = (ShaderMaterial)Material;
    }
    
    public static BeamBuilder Create()
    {
        var beam = Scene.Instantiate<Beam>();
        return new BeamBuilder(beam);
    }

    public override void _Ready()
    {
        
    }

    private static Vector2 PixelsToUV(Vector2 pixelPosition)
    {
        return new Vector2(
            pixelPosition.X / BaseSize.X,
            pixelPosition.Y / BaseSize.Y
        );
    }

    private float PixelsToRelativeWidth(float pixelWidth)
    {
        var referenceDimension = Min(BaseSize.X, BaseSize.Y);
        return pixelWidth / referenceDimension;
    }

    public void SetFromTo(Vector2 from, Vector2 to)
    {
        SetAnchorsPreset(LayoutPreset.TopLeft);
        Size = BaseSize;
        Position = from - new Vector2(0, BaseSize.Y / 2);
        Rotation = from.AngleToPoint(to);
        var length = from.DistanceTo(to) / Size.X;
        SetLength(length);

        const float fadeDistanceToLengthFactor = 0.05f;
        SetFadeDistance(length * fadeDistanceToLengthFactor);
    }

    public void SetBeamCount(int count) => _shaderMaterial.SetShaderParameter("beams", count);
    public void SetEnergy(float energy) => _shaderMaterial.SetShaderParameter("energy", energy);
    public void SetRoughness(int roughness) => _shaderMaterial.SetShaderParameter("roughness", roughness);
    public void SetFrequency(int frequency) => _shaderMaterial.SetShaderParameter("frequency", frequency);
    public void SetSpeed(float speed) => _shaderMaterial.SetShaderParameter("speed", speed);
    public void SetProgress(float progress) => _shaderMaterial.SetShaderParameter("progress", progress);

    public void SetCoreWidthPixels(float widthPixels)
    {
        _shaderMaterial.SetShaderParameter("beam_core_width", PixelsToRelativeWidth(widthPixels));
    }

    public void SetOutlineWidthPixels(float widthPixels)
    {
        _shaderMaterial.SetShaderParameter("beam_outline_width", PixelsToRelativeWidth(widthPixels));
    }

    public void SetBeamDifference(float difference) =>
        _shaderMaterial.SetShaderParameter("beam_difference", difference);

    public void SetGlow(float glow) => _shaderMaterial.SetShaderParameter("glow", glow);
    public void SetOutlineGlow(float glow) => _shaderMaterial.SetShaderParameter("outline_glow", glow);
    public void SetBeamColor(Color color) => _shaderMaterial.SetShaderParameter("color", color);
    public void SetOutlineColor(Color color) => _shaderMaterial.SetShaderParameter("outline_color", color);
    public void SetLength(float length) => _shaderMaterial.SetShaderParameter("beam_length", length);
    public void SetFadeDistance(float distance) => _shaderMaterial.SetShaderParameter("fade_distance", distance);

    public ShaderMaterial GetShaderMaterial() => _shaderMaterial;
}

public class BeamBuilder
{
    private readonly Beam _beam;

    private Vector2? _fromPixels;
    private Vector2? _toPixels;
    private int _beamCount = 2;
    private float _energy = 3.0f;
    private int _roughness = 3;
    private int _frequency = 10;
    private float _speed = 1.0f;
    private float _coreWidthPixels = 20.0f;
    private float _outlineWidthPixels = 60.0f;
    private float _beamDifference = 0.0f;
    private float _glow = 0.0f;
    private float _outlineGlow = 0.0f;
    private Color _color = new Color(0.91f, 1.0f, 1.0f, 1.0f);
    private Color _outlineColor = new Color(0.5f, 1.0f, 0.96f, 1.0f);
    private float _progress = 1.0f;
    private float _edgeFadeSize = 0.05f;
    private float _endFadeSize = 0.1f;

    internal BeamBuilder(Beam beam)
    {
        _beam = beam;
    }

    public BeamBuilder FromTo(Vector2 fromPixels, Vector2 toPixels)
    {
        _fromPixels = fromPixels;
        _toPixels = toPixels;
        return this;
    }

    public BeamBuilder From(Vector2 fromPixels)
    {
        _fromPixels = fromPixels;
        return this;
    }

    public BeamBuilder To(Vector2 toPixels)
    {
        _toPixels = toPixels;
        return this;
    }

    public BeamBuilder WithBeamCount(int count)
    {
        _beamCount = count;
        return this;
    }

    public BeamBuilder WithEnergy(float energy)
    {
        _energy = energy;
        return this;
    }

    public BeamBuilder WithRoughness(int roughness)
    {
        _roughness = Clamp(roughness, 1, 10);
        return this;
    }

    public BeamBuilder WithFrequency(int frequency)
    {
        _frequency = frequency;
        return this;
    }

    public BeamBuilder WithSpeed(float speed)
    {
        _speed = speed;
        return this;
    }

    public BeamBuilder WithCoreWidth(float widthPixels)
    {
        _coreWidthPixels = widthPixels;
        return this;
    }

    public BeamBuilder WithOutlineWidth(float widthPixels)
    {
        _outlineWidthPixels = widthPixels;
        return this;
    }

    public BeamBuilder WithBeamDifference(float difference)
    {
        _beamDifference = Clamp(difference, 0.0f, 1.0f);
        return this;
    }

    public BeamBuilder WithGlow(float glow)
    {
        _glow = Clamp(glow, 0.0f, 3.0f);
        return this;
    }

    public BeamBuilder WithOutlineGlow(float glow)
    {
        _outlineGlow = Clamp(glow, 0.0f, 3.0f);
        return this;
    }

    public BeamBuilder WithColor(Color color)
    {
        _color = color;
        return this;
    }

    public BeamBuilder WithOutlineColor(Color color)
    {
        _outlineColor = color;
        return this;
    }

    public BeamBuilder WithProgress(float progress)
    {
        _progress = Clamp(progress, 0.0f, 1.0f);
        return this;
    }

    public BeamBuilder WithEdgeFade(float fadeSize)
    {
        _edgeFadeSize = Clamp(fadeSize, 0.0f, 0.5f);
        return this;
    }

    public BeamBuilder WithEndFade(float fadeSize)
    {
        _endFadeSize = Clamp(fadeSize, 0.0f, 0.5f);
        return this;
    }

    public BeamBuilder ElectricPreset()
    {
        return WithColor(new Color(0.7f, 0.9f, 1.0f, 1.0f))
            .WithOutlineColor(new Color(0.3f, 0.6f, 1.0f, 1.0f))
            .WithEnergy(4.0f)
            .WithFrequency(15)
            .WithSpeed(2.0f)
            .WithGlow(1.5f)
            .WithOutlineGlow(0.8f);
    }

    public BeamBuilder LaserPreset()
    {
        return WithColor(new Color(1.0f, 0.2f, 0.2f, 1.0f))
            .WithOutlineColor(new Color(1.0f, 0.6f, 0.0f, 1.0f))
            .WithEnergy(1.0f)
            .WithFrequency(5)
            .WithSpeed(0.5f)
            .WithBeamCount(1)
            .WithGlow(2.0f);
    }

    public BeamBuilder MagicPreset()
    {
        return WithColor(new Color(0.8f, 0.2f, 1.0f, 1.0f))
            .WithOutlineColor(new Color(1.0f, 0.4f, 0.8f, 1.0f))
            .WithEnergy(5.0f)
            .WithFrequency(20)
            .WithSpeed(1.5f)
            .WithBeamCount(3)
            .WithGlow(1.0f)
            .WithOutlineGlow(1.5f);
    }

    public Beam Build()
    {
        if (!_fromPixels.HasValue || !_toPixels.HasValue)
        {
            GD.PrintErr("BeamBuilder: From and To positions must be set before building!");
            return _beam;
        }

        _beam.SetFromTo(_fromPixels.Value, _toPixels.Value);
        _beam.SetBeamCount(_beamCount);
        _beam.SetEnergy(_energy);
        _beam.SetRoughness(_roughness);
        _beam.SetFrequency(_frequency);
        _beam.SetSpeed(_speed);
        _beam.SetCoreWidthPixels(_coreWidthPixels);
        _beam.SetOutlineWidthPixels(_outlineWidthPixels);
        _beam.SetBeamDifference(_beamDifference);
        _beam.SetGlow(_glow);
        _beam.SetOutlineGlow(_outlineGlow);
        _beam.SetBeamColor(_color);
        _beam.SetOutlineColor(_outlineColor);
        _beam.SetProgress(_progress);

        return _beam;
    }
}
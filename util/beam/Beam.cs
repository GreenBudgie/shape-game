using Godot;

public partial class Beam : ColorRect
{

    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://g21wng36wpva");
    
    public static BeamBuilder Create() => new BeamBuilder();

    public class BeamBuilder
    {
        private Vector2 _from;
        private Vector2 _to;
        private Color _color = new(0.91f, 1f, 1f);
        private Color _outlineColor = new(0.5f, 1f, 0.96f);
        private float _thickness = 0.006f;
        private float _outlineThickness = 0.03f;
        private int _beams = 2;
        private float _energy = 3.0f;
        private int _roughness = 3;
        private int _frequency = 10;
        private float _speed = 1.0f;
        private float _beamDifference;
        private float _glow;
        private float _outlineGlow;
        private float _progress = 1.0f;
        private float _yOffset;
        private float _fixedEdgeSize = 0.05f;
        private Vector2 _noiseScale = new(1f, 1f);

        public BeamBuilder From(Vector2 point)
        {
            _from = point;
            return this;
        }

        public BeamBuilder To(Vector2 point)
        {
            _to = point;
            return this;
        }

        public BeamBuilder Color(Color color)
        {
            _color = color;
            return this;
        }
        
        public BeamBuilder OutlineColor(Color outlineColor)
        {
            _outlineColor = outlineColor;
            return this;
        }

        public BeamBuilder Thickness(float thickness)
        {
            _thickness = thickness;
            return this;
        }

        public BeamBuilder OutlineThickness(float thickness)
        {
            _outlineThickness = thickness;
            return this;
        }

        public BeamBuilder Beams(int beams)
        {
            _beams = beams;
            return this;
        }

        public BeamBuilder Energy(float energy)
        {
            _energy = energy;
            return this;
        }

        public BeamBuilder Roughness(int roughness)
        {
            _roughness = roughness;
            return this;
        }

        public BeamBuilder Frequency(int frequency)
        {
            _frequency = frequency;
            return this;
        }

        public BeamBuilder Speed(float speed)
        {
            _speed = speed;
            return this;
        }

        public BeamBuilder BeamDifference(float difference)
        {
            _beamDifference = difference;
            return this;
        }

        public BeamBuilder Glow(float glow)
        {
            _glow = glow;
            return this;
        }

        public BeamBuilder OutlineGlow(float outlineGlow)
        {
            _outlineGlow = outlineGlow;
            return this;
        }

        public BeamBuilder Progress(float progress)
        {
            _progress = progress;
            return this;
        }

        public BeamBuilder YOffset(float yOffset)
        {
            _yOffset = yOffset;
            return this;
        }

        public BeamBuilder FixedEdgeSize(float size)
        {
            _fixedEdgeSize = size;
            return this;
        }

        public BeamBuilder NoiseScale(Vector2 scale)
        {
            _noiseScale = scale;
            return this;
        }

        public Beam Build()
        {
            var beam = Scene.Instantiate<Beam>();
            var material = (ShaderMaterial)beam.Material;
            material.SetShaderParameter("beams", _beams);
            material.SetShaderParameter("energy", _energy);
            material.SetShaderParameter("roughness", _roughness);
            material.SetShaderParameter("frequency", _frequency);
            material.SetShaderParameter("speed", _speed);
            material.SetShaderParameter("thickness", _thickness);
            material.SetShaderParameter("outline_thickness", _outlineThickness);
            material.SetShaderParameter("beam_difference", _beamDifference);
            material.SetShaderParameter("glow", _glow);
            material.SetShaderParameter("outline_glow", _outlineGlow);
            material.SetShaderParameter("color", _color);
            material.SetShaderParameter("outline_color", _outlineColor);
            material.SetShaderParameter("progress", _progress);
            material.SetShaderParameter("y_offset", _yOffset);
            material.SetShaderParameter("fixed_edge_size", _fixedEdgeSize);
            material.SetShaderParameter("noise_scale", _noiseScale);

            // Adjust size and position based on from/to points
            var direction = _to - _from;
            var distance = direction.Length();
            var angle = direction.Angle();
            beam.Position = _from;
            beam.Rotation = angle;
            var totalThickness = (_thickness + _outlineThickness) * 10000f; // Scale UV units to pixels (adjust 100f as needed)
            beam.Size = new Vector2(distance, totalThickness);

            return beam;
        }
    }
}
public class GlowTween
{
    private Tween? _tween;
    
    // Configuration properties
    private float _strengthDelta = 0.5f;
    private float _maxStrength = 2.0f;
    private float _radiusDelta = 10f;
    private float _maxRadius = 50f;
    private float _minStrength = 0f;
    private float _minRadius = 0f;
    private float _inTime = 0.2f;
    private float _outTime = 0.3f;

    public static GlowTween From(GlowTween other)
    {
        return new GlowTween
        {
            _strengthDelta = other._strengthDelta,
            _maxStrength = other._maxStrength,
            _radiusDelta = other._radiusDelta,
            _maxRadius = other._maxRadius,
            _minStrength = other._minStrength,
            _minRadius = other._minRadius,
            _inTime = other._inTime,
            _outTime = other._outTime
        };
    }
    
    public GlowTween StrengthDelta(float delta)
    {
        _strengthDelta = delta;
        return this;
    }

    public GlowTween MaxStrength(float maxStrength)
    {
        _maxStrength = maxStrength;
        return this;
    }

    public GlowTween RadiusDelta(float delta)
    {
        _radiusDelta = delta;
        return this;
    }

    public GlowTween MaxRadius(float maxRadius)
    {
        _maxRadius = maxRadius;
        return this;
    }

    public GlowTween MinStrength(float minStrength)
    {
        _minStrength = minStrength;
        return this;
    }

    public GlowTween MinRadius(float minRadius)
    {
        _minRadius = minRadius;
        return this;
    }

    public GlowTween InTime(float time)
    {
        _inTime = time;
        return this;
    }

    public GlowTween OutTime(float time)
    {
        _outTime = time;
        return this;
    }

    public GlowTween Times(float inTime, float outTime)
    {
        _inTime = inTime;
        _outTime = outTime;
        return this;
    }

    public GlowTween MinValues(float minStrength, float minRadius)
    {
        _minStrength = minStrength;
        _minRadius = minRadius;
        return this;
    }

    public GlowTween MaxValues(float maxStrength, float maxRadius)
    {
        _maxStrength = maxStrength;
        _maxRadius = maxRadius;
        return this;
    }

    public GlowTween Deltas(float strengthDelta, float radiusDelta)
    {
        _strengthDelta = strengthDelta;
        _radiusDelta = radiusDelta;
        return this;
    }

    public GlowTween Play<T>(T target) where T : Node, IGlow
    {
        _tween?.Kill();
        _tween = target.CreateTween().SetTrans(Tween.TransitionType.Sine);

        var cappedStrength = Min(target.Strength + _strengthDelta, _maxStrength);
        var cappedRadius = Min(target.Radius + _radiusDelta, _maxRadius);

        _tween.TweenProperty(target, IGlow.StrengthProperty, cappedStrength, _inTime)
            .SetEase(Tween.EaseType.Out);
        _tween.Parallel().TweenProperty(target, IGlow.RadiusProperty, cappedRadius, _inTime)
            .SetEase(Tween.EaseType.Out);

        _tween.TweenProperty(target, IGlow.StrengthProperty, _minStrength, _outTime)
            .SetEase(Tween.EaseType.In);
        _tween.Parallel().TweenProperty(target, IGlow.RadiusProperty, _minRadius, _outTime)
            .SetEase(Tween.EaseType.In);

        return this;
    }
}
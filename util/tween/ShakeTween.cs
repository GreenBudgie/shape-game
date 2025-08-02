public class ShakeTween
{
    private Tween? _tween;
    
    // Configuration properties
    private float _tiltDegDelta = 20f;
    private float _maxTiltDeg = 45f;
    private float _sizeDelta = 0.2f;
    private float _maxSize = 1.6f;
    private float _inTime = 0.08f;
    private float _outTime = 0.2f;

    public ShakeTween TiltDelta(float degrees)
    {
        _tiltDegDelta = degrees;
        return this;
    }

    public ShakeTween MaxTilt(float maxDegrees)
    {
        _maxTiltDeg = maxDegrees;
        return this;
    }

    public ShakeTween SizeDelta(float delta)
    {
        _sizeDelta = delta;
        return this;
    }

    public ShakeTween MaxSize(float maxSize)
    {
        _maxSize = maxSize;
        return this;
    }

    public ShakeTween InTime(float time)
    {
        _inTime = time;
        return this;
    }

    public ShakeTween OutTime(float time)
    {
        _outTime = time;
        return this;
    }

    public ShakeTween Times(float inTime, float outTime)
    {
        _inTime = inTime;
        _outTime = outTime;
        return this;
    }

    public ShakeTween Play(Control target)
    {
        // Kill any existing tween
        _tween?.Kill();
        
        // Create new tween
        _tween = target.CreateTween();

        // Calculate target values
        var randomDirTilt = GD.Randf() > 0.5 ? -_tiltDegDelta : _tiltDegDelta;
        var cappedTilt = Min(target.RotationDegrees + randomDirTilt, _maxTiltDeg);
        var cappedSize = Min(target.Scale.X + _sizeDelta, _maxSize);

        // Tween in (shake)
        _tween.TweenProperty(
            @object: target,
            property: Control.PropertyName.RotationDegrees.ToString(),
            finalVal: cappedTilt,
            duration: _inTime
        ).SetEase(Tween.EaseType.In);

        _tween.Parallel().TweenProperty(
            @object: target,
            property: Control.PropertyName.Scale.ToString(),
            finalVal: new Vector2(cappedSize, cappedSize),
            duration: _inTime
        ).SetEase(Tween.EaseType.In);

        // Tween out (return to normal)
        _tween.TweenProperty(
            @object: target,
            property: Control.PropertyName.RotationDegrees.ToString(),
            finalVal: 0,
            duration: _outTime
        ).SetEase(Tween.EaseType.Out);

        _tween.Parallel().TweenProperty(
            @object: target,
            property: Control.PropertyName.Scale.ToString(),
            finalVal: Vector2.One,
            duration: _outTime
        ).SetEase(Tween.EaseType.Out);

        return this;
    }

}
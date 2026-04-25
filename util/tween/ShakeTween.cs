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

    private float _positionShakeMagnitude;
    private float _sizeShakeMagnitude;
    private float _rotationShakeMagnitude;
    private float _shakeFrequencyHz = 30f;

    public static ShakeTween From(ShakeTween other)
    {
        return new ShakeTween
        {
            _tiltDegDelta = other._tiltDegDelta,
            _maxTiltDeg = other._maxTiltDeg,
            _sizeDelta = other._sizeDelta,
            _maxSize = other._maxSize,
            _inTime = other._inTime,
            _outTime = other._outTime,
            _positionShakeMagnitude = other._positionShakeMagnitude,
            _sizeShakeMagnitude = other._sizeShakeMagnitude,
            _rotationShakeMagnitude = other._rotationShakeMagnitude,
            _shakeFrequencyHz = other._shakeFrequencyHz
        };
    }
    
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
    
    public ShakeTween FixedSize(float size)
    {
        _maxSize = 1 + size;
        _sizeDelta = size;
        return this;
    }
    
    public ShakeTween FixedTilt(float tiltDeg)
    {
        _maxTiltDeg = tiltDeg;
        _tiltDegDelta = tiltDeg;
        return this;
    }

    /// <summary>
    /// Max random position offset in pixels. 0 disables position shake.
    /// </summary>
    public ShakeTween PositionShakeMagnitude(float magnitude)
    {
        _positionShakeMagnitude = magnitude;
        return this;
    }

    /// <summary>
    /// Max random scale offset (added to the scale factor). 0 disables size shake.
    /// </summary>
    public ShakeTween SizeShakeMagnitude(float magnitude)
    {
        _sizeShakeMagnitude = magnitude;
        return this;
    }

    /// <summary>
    /// Max random rotation offset in degrees. 0 disables rotation shake.
    /// </summary>
    public ShakeTween RotationShakeMagnitude(float magnitude)
    {
        _rotationShakeMagnitude = magnitude;
        return this;
    }

    public ShakeTween ShakeFrequency(float hz)
    {
        _shakeFrequencyHz = hz;
        return this;
    }

    public ShakeTween Play(Node2D target)
    {
        return DoPlay(target, target.Position, target.RotationDegrees, target.Scale);
    }

    public ShakeTween Play(Control target)
    {
        return DoPlay(target, target.Position, target.RotationDegrees, target.Scale);
    }

    private ShakeTween DoPlay(CanvasItem target, Vector2 basePosition, float baseRotationDegrees, Vector2 baseScale)
    {
        _tween?.Kill();
        var tween = target.CreateTween().SetTrans(Tween.TransitionType.Sine);
        _tween = tween;

        var randomDirTilt = GD.Randf() > 0.5 ? -_tiltDegDelta : _tiltDegDelta;
        var peakTilt = Min(baseRotationDegrees + randomDirTilt, _maxTiltDeg);
        var peakScaleFactor = Min(baseScale.X + _sizeDelta, _maxSize);

        var hasShake = _positionShakeMagnitude > 0
                       || _sizeShakeMagnitude > 0
                       || _rotationShakeMagnitude > 0;

        if (hasShake)
        {
            PlayWithShake(tween, target, basePosition, baseRotationDegrees, baseScale, peakTilt, peakScaleFactor);
        }
        else
        {
            PlayPunchOnly(tween, target, baseRotationDegrees, baseScale, peakTilt, peakScaleFactor);
        }

        return this;
    }

    private void PlayPunchOnly(Tween tween, CanvasItem target, float baseRotationDegrees, Vector2 baseScale, float peakTilt, float peakScaleFactor)
    {
        tween.TweenProperty(target, RotationDegreesProperty, peakTilt, _inTime)
            .SetEase(Tween.EaseType.In);
        tween.Parallel().TweenProperty(target, ScaleProperty, new Vector2(peakScaleFactor, peakScaleFactor), _inTime)
            .SetEase(Tween.EaseType.In);

        tween.TweenProperty(target, RotationDegreesProperty, baseRotationDegrees, _outTime)
            .SetEase(Tween.EaseType.Out);
        tween.Parallel().TweenProperty(target, ScaleProperty, baseScale, _outTime)
            .SetEase(Tween.EaseType.Out);
    }

    private void PlayWithShake(Tween tween, CanvasItem target, Vector2 basePosition, float baseRotationDegrees, Vector2 baseScale, float peakTilt, float peakScaleFactor)
    {
        var totalDuration = _inTime + _outTime;
        var steps = Max(4, RoundToInt(totalDuration * _shakeFrequencyHz));
        var stepDuration = totalDuration / steps;
        var inFraction = totalDuration > 0 ? _inTime / totalDuration : 0.5f;

        for (var i = 1; i <= steps; i++)
        {
            var t = (float)i / steps;

            // Triangular envelope: ramp up to 1 at end of "in" phase, back to 0 at end of "out" phase.
            float punchEnvelope;
            if (t <= inFraction)
            {
                punchEnvelope = inFraction > 0 ? t / inFraction : 1f;
            }
            else
            {
                punchEnvelope = 1f - (t - inFraction) / (1f - inFraction);
            }

            // Shake intensity fades linearly so the motion settles as the punch resolves.
            var shakeIntensity = 1f - t;

            var currentRotation = Lerp(baseRotationDegrees, peakTilt, punchEnvelope);
            var currentScaleFactor = Lerp(baseScale.X, peakScaleFactor, punchEnvelope);

            var positionMagnitude = _positionShakeMagnitude * shakeIntensity;
            var positionOffset = new Vector2(
                RandomUtils.DeltaRange(0f, positionMagnitude),
                RandomUtils.DeltaRange(0f, positionMagnitude)
            );
            var scaleOffset = RandomUtils.DeltaRange(0f, _sizeShakeMagnitude * shakeIntensity);
            var rotationOffset = RandomUtils.DeltaRange(0f, _rotationShakeMagnitude * shakeIntensity);

            var stepScale = currentScaleFactor + scaleOffset;

            tween.TweenProperty(target, PositionProperty, basePosition + positionOffset, stepDuration);
            tween.Parallel().TweenProperty(target, RotationDegreesProperty, currentRotation + rotationOffset, stepDuration);
            tween.Parallel().TweenProperty(target, ScaleProperty, new Vector2(stepScale, stepScale), stepDuration);
        }

        // Snap cleanly back to the base values so repeated plays don't drift.
        tween.TweenProperty(target, PositionProperty, basePosition, stepDuration);
        tween.Parallel().TweenProperty(target, RotationDegreesProperty, baseRotationDegrees, stepDuration);
        tween.Parallel().TweenProperty(target, ScaleProperty, baseScale, stepDuration);
    }

}
public class ShakeTween
{
    private Tween? _tween;

    // Configuration properties
    private float _tiltDegDelta;
    private float _maxTiltDeg;
    private float _sizeDelta;
    private float _maxSize;
    private float _inTime;
    private float _outTime;

    private float _positionShakeMagnitude;
    private float _sizeShakeMagnitude;
    private float _rotationShakeMagnitude;
    private float _shakeFrequencyHz = 30f;

    private Vector2 _initialPosition;
    private float _initialRotationDegrees;
    private Vector2 _initialScale;

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
        if (_tween != null)
        {
            target.Position = _initialPosition;
            target.RotationDegrees = _initialRotationDegrees;
            target.Scale = _initialScale;
        }
        
        return DoPlay(target, target.Position, target.RotationDegrees, target.Scale);
    }

    public ShakeTween Play(Control target)
    {
        if (_tween != null)
        {
            target.Position = _initialPosition;
            target.RotationDegrees = _initialRotationDegrees;
            target.Scale = _initialScale;
        }
        
        return DoPlay(target, target.Position, target.RotationDegrees, target.Scale);
    }

    private ShakeTween DoPlay(CanvasItem target, Vector2 basePosition, float baseRotationDegrees, Vector2 baseScale)
    {
        _tween?.Kill();
        _tween = null;

        var animatesPosition = _positionShakeMagnitude > 0;
        var animatesRotation = _tiltDegDelta != 0 || _rotationShakeMagnitude > 0;
        var animatesScale = _sizeDelta != 0 || _sizeShakeMagnitude > 0;

        if (!animatesPosition && !animatesRotation && !animatesScale)
        {
            return this;
        }

        _initialPosition = basePosition;
        _initialRotationDegrees = baseRotationDegrees;
        _initialScale = baseScale;

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
            PlayWithShake(tween, target, basePosition, baseRotationDegrees, baseScale, peakTilt, peakScaleFactor,
                animatesPosition, animatesRotation, animatesScale);
        }
        else
        {
            PlayPunchOnly(tween, target, baseRotationDegrees, baseScale, peakTilt, peakScaleFactor,
                animatesRotation, animatesScale);
        }

        return this;
    }

    private void PlayPunchOnly(Tween tween, CanvasItem target, float baseRotationDegrees, Vector2 baseScale, float peakTilt, float peakScaleFactor,
        bool animatesRotation, bool animatesScale)
    {
        AddPunchPhase(tween, target, _inTime, Tween.EaseType.In,
            animatesRotation, peakTilt,
            animatesScale, new Vector2(peakScaleFactor, peakScaleFactor));

        AddPunchPhase(tween, target, _outTime, Tween.EaseType.Out,
            animatesRotation, baseRotationDegrees,
            animatesScale, baseScale);
    }

    private static void AddPunchPhase(Tween tween, CanvasItem target, float duration, Tween.EaseType ease,
        bool animatesRotation, float rotationVal,
        bool animatesScale, Vector2 scaleVal)
    {
        var isFirst = true;
        if (animatesRotation)
        {
            AddProperty(tween, ref isFirst, target, RotationDegreesProperty, rotationVal, duration).SetEase(ease);
        }
        if (animatesScale)
        {
            AddProperty(tween, ref isFirst, target, ScaleProperty, scaleVal, duration).SetEase(ease);
        }
    }

    private void PlayWithShake(Tween tween, CanvasItem target, Vector2 basePosition, float baseRotationDegrees, Vector2 baseScale, float peakTilt, float peakScaleFactor,
        bool animatesPosition, bool animatesRotation, bool animatesScale)
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

            var isFirst = true;

            if (animatesPosition)
            {
                var positionMagnitude = _positionShakeMagnitude * shakeIntensity;
                var positionOffset = new Vector2(
                    RandomUtils.DeltaRange(0f, positionMagnitude),
                    RandomUtils.DeltaRange(0f, positionMagnitude)
                );
                AddProperty(tween, ref isFirst, target, PositionProperty, basePosition + positionOffset, stepDuration);
            }

            if (animatesRotation)
            {
                var currentRotation = Lerp(baseRotationDegrees, peakTilt, punchEnvelope);
                var rotationOffset = RandomUtils.DeltaRange(0f, _rotationShakeMagnitude * shakeIntensity);
                AddProperty(tween, ref isFirst, target, RotationDegreesProperty, currentRotation + rotationOffset, stepDuration);
            }

            if (animatesScale)
            {
                var currentScaleFactor = Lerp(baseScale.X, peakScaleFactor, punchEnvelope);
                var scaleOffset = RandomUtils.DeltaRange(0f, _sizeShakeMagnitude * shakeIntensity);
                var stepScale = currentScaleFactor + scaleOffset;
                AddProperty(tween, ref isFirst, target, ScaleProperty, new Vector2(stepScale, stepScale), stepDuration);
            }
        }

        // Snap cleanly back to the base values so repeated plays don't drift.
        var snapFirst = true;
        if (animatesPosition)
        {
            AddProperty(tween, ref snapFirst, target, PositionProperty, basePosition, stepDuration);
        }
        if (animatesRotation)
        {
            AddProperty(tween, ref snapFirst, target, RotationDegreesProperty, baseRotationDegrees, stepDuration);
        }
        if (animatesScale)
        {
            AddProperty(tween, ref snapFirst, target, ScaleProperty, baseScale, stepDuration);
        }
    }

    private static PropertyTweener AddProperty(Tween tween, ref bool isFirst, GodotObject target, NodePath property, Variant finalVal, float duration)
    {
        var tweener = isFirst
            ? tween.TweenProperty(target, property, finalVal, duration)
            : tween.Parallel().TweenProperty(target, property, finalVal, duration);
        isFirst = false;
        return tweener;
    }

}
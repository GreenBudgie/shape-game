using System.Collections.Generic;

/// <summary>
/// Makes fire burn all over the area of its host. Ignites flames at random points inside the collision shapes
/// and keeps replacing burnt out flames with new ones at other points, so the fire looks alive everywhere.
/// The number and the size of flames are derived from the area of the host, every flame gets slightly random
/// parameters and timings.
/// <br/><br/>
/// Flames always rise up, whatever the rotation of the host, and lean against the movement of the host.
/// </summary>
public partial class FireDisplay : Node2D
{

    // Average flame quad size is SizeScale * area^SizeExponent. The exponent below 0.5 makes flames grow
    // with the area, but slower than the area itself, so big hosts are covered by more flames, not only bigger ones.
    // Areas of current hosts: player ~6900, polysteroids ~11000-30000, square ~32000, rectangle ~77000.
    private const float SizeScale = 3.6f;
    private const float SizeExponent = 0.4f;
    private const float MinFlameSize = 48f;
    private const float MaxFlameSize = 340f;

    /// <summary>
    /// Approximate part of a flame quad covered by the visible flame.
    /// </summary>
    private const float VisibleFlameFraction = 0.35f;
    /// <summary>
    /// How many times the visible flames together cover the area. Above 1, so flames overlap and leave no holes.
    /// </summary>
    private const float Coverage = 2f;
    private const int MinFlames = 1;
    private const int MaxFlames = 8;

    /// <summary>
    /// How many random points are tried for a new flame. The point farthest from other burning flames is taken,
    /// so flames spread over the area instead of crowding.
    /// </summary>
    private const int PointCandidates = 5;

    // Random ranges of flame parameters, around the defaults of the fire shader
    private const float MinSizeFactor = 0.75f;
    private const float MaxSizeFactor = 1.15f;
    private const float MinIntensity = 0.8f;
    private const float MaxIntensity = 1.15f;
    private const float MinGlowStrength = 0.35f;
    private const float MaxGlowStrength = 0.65f;
    private const float MinSpeed = 1.2f;
    private const float MaxSpeed = 1.9f;
    private const float MinFlameHeight = 0.5f;
    private const float MaxFlameHeight = 0.9f;
    private const float MinTurbulence = 0.6f;
    private const float MaxTurbulence = 0.8f;
    private const float MinDetail = 2.6f;
    private const float MaxDetail = 3.4f;

    // Random ranges of the flame lifecycle, in seconds
    private const float MinIgniteDuration = 0.2f;
    private const float MaxIgniteDuration = 0.45f;
    private const float MinBurnDuration = 0.15f;
    private const float MaxBurnDuration = 0.8f;
    private const float MinExtinguishDuration = 0.25f;
    private const float MaxExtinguishDuration = 0.5f;
    /// <summary>
    /// A replacement flame ignites after a random delay up to this, once the previous flame starts burning out.
    /// </summary>
    private const float MaxReplacementDelay = 0.15f;
    /// <summary>
    /// The first flames ignite at random moments within this time, so they do not burn in sync.
    /// </summary>
    private const float InitialIgniteSpread = 0.6f;

    /// <summary>
    /// Speed of a flame, in pixels per second, at which it reaches the maximum lean.
    /// </summary>
    private const float SpeedForMaxLean = 1500f;
    /// <summary>
    /// How fast the lean follows the movement. Higher = more responsive, lower = smoother.
    /// </summary>
    private const float LeanSmoothing = 8f;
    /// <summary>
    /// Maximum drift of the noise pattern caused by the movement, in flame units per second.
    /// Keeps fast flames from turning into flicker.
    /// </summary>
    private const float MaxDriftSpeed = 3f;
    /// <summary>
    /// Random noise offsets of flames are picked in this range, in flame units.
    /// </summary>
    private const float NoiseOffsetRange = 100f;

    private readonly List<Flame> _flames = [];

    private IAreaAware _host = null!;
    private Node2D _hostNode = null!;
    private Color? _color;
    private float _flameSize;
    private bool _isStopping;

    /// <summary>
    /// Color of all flames, or null if flames use the default palette of the fire shader.
    /// </summary>
    public Color? Color => _color;

    /// <summary>
    /// Creates a fire display and adds it as a child of the host, so it moves and gets removed together with the host.
    /// </summary>
    public static FireDisplay Attach(IAreaAware host)
    {
        var display = new FireDisplay();
        display._host = host;
        display._hostNode = host.Area.CollisionObject;
        display._hostNode.AddChild(display);
        return display;
    }

    public override void _Ready()
    {
        GlobalRotation = 0;

        var area = _host.Area.Area;
        _flameSize = Clamp(SizeScale * Pow(area, SizeExponent), MinFlameSize, MaxFlameSize);

        var visibleFlameArea = VisibleFlameFraction * _flameSize * _flameSize;
        var flameCount = Clamp(CeilToInt(Coverage * area / visibleFlameArea), MinFlames, MaxFlames);

        for (var i = 0; i < flameCount; i++)
        {
            IgniteFlameAfter(GD.Randf() * InitialIgniteSpread);
        }
    }

    public override void _Process(double delta)
    {
        GlobalRotation = 0;

        foreach (var flame in _flames)
        {
            UpdateFlame(flame, (float)delta);
        }
    }

    /// <summary>
    /// Sets the color of all current and future flames. See <see cref="Fire.SetFlameColor"/>.
    /// </summary>
    public FireDisplay SetColor(Color color)
    {
        _color = color;
        foreach (var flame in _flames)
        {
            flame.Fire.SetFlameColor(color);
        }

        return this;
    }

    /// <summary>
    /// Extinguishes all flames, stops igniting new ones and removes the display when the last flame is out.
    /// </summary>
    public void Stop()
    {
        if (_isStopping)
        {
            return;
        }

        _isStopping = true;
        foreach (var flame in _flames)
        {
            if (!flame.IsBurningOut)
            {
                flame.Tween?.Kill();
                BurnOut(flame);
            }
        }

        if (_flames.Count == 0)
        {
            QueueFree();
        }
    }

    private void IgniteFlameAfter(float delay)
    {
        var tween = CreateTween();
        tween.TweenInterval(delay);
        tween.TweenCallback(Callable.From(IgniteFlame));
    }

    private void IgniteFlame()
    {
        if (_isStopping)
        {
            return;
        }

        var fire = Fire.Create()
            .SetFlameSize(_flameSize * RandomUtils.Range(MinSizeFactor, MaxSizeFactor))
            .SetNoiseOffset(new Vector2(GD.Randf(), GD.Randf()) * NoiseOffsetRange)
            .SetIntensity(RandomUtils.Range(MinIntensity, MaxIntensity))
            .SetGlowStrength(RandomUtils.Range(MinGlowStrength, MaxGlowStrength))
            .SetSpeed(RandomUtils.Range(MinSpeed, MaxSpeed))
            .SetFlameHeight(RandomUtils.Range(MinFlameHeight, MaxFlameHeight))
            .SetTurbulence(RandomUtils.Range(MinTurbulence, MaxTurbulence))
            .SetDetail(RandomUtils.Range(MinDetail, MaxDetail));
        if (_color.HasValue)
        {
            fire.SetFlameColor(_color.Value);
        }

        var flame = new Flame(fire, PickFlamePoint());
        _flames.Add(flame);
        AddChild(fire);

        var offset = GetFlameOffset(flame);
        fire.SetCenter(offset);
        flame.PrevGlobalPosition = GlobalPosition + offset;

        // Tweens of a node can be created only inside the tree, so the fire is added first
        var tween = fire.Ignite(RandomUtils.Range(MinIgniteDuration, MaxIgniteDuration));
        tween.TweenInterval(RandomUtils.Range(MinBurnDuration, MaxBurnDuration));
        tween.TweenCallback(Callable.From(() => BurnOut(flame)));
        flame.Tween = tween;
    }

    /// <summary>
    /// Picks a random point inside the host, the farthest one from other burning flames among a few candidates.
    /// </summary>
    private Vector2 PickFlamePoint()
    {
        var bestPoint = _host.Area.GetRandomLocalPoint();
        var bestDistance = GetDistanceToBurningFlames(bestPoint);

        for (var i = 1; i < PointCandidates; i++)
        {
            var point = _host.Area.GetRandomLocalPoint();
            var distance = GetDistanceToBurningFlames(point);
            if (distance > bestDistance)
            {
                bestPoint = point;
                bestDistance = distance;
            }
        }

        return bestPoint;
    }

    private float GetDistanceToBurningFlames(Vector2 point)
    {
        var minDistanceSquared = float.MaxValue;
        foreach (var flame in _flames)
        {
            if (!flame.IsBurningOut)
            {
                minDistanceSquared = Min(minDistanceSquared, point.DistanceSquaredTo(flame.LocalPoint));
            }
        }

        return minDistanceSquared;
    }

    /// <summary>
    /// Starts extinguishing the flame and ignites a replacement at another point.
    /// </summary>
    private void BurnOut(Flame flame)
    {
        flame.IsBurningOut = true;

        var tween = flame.Fire.Extinguish(RandomUtils.Range(MinExtinguishDuration, MaxExtinguishDuration));
        tween.TweenCallback(Callable.From(() => RemoveFlame(flame)));
        flame.Tween = tween;

        if (!_isStopping)
        {
            IgniteFlameAfter(GD.Randf() * MaxReplacementDelay);
        }
    }

    private void RemoveFlame(Flame flame)
    {
        _flames.Remove(flame);
        flame.Fire.QueueFree();

        if (_isStopping && _flames.Count == 0)
        {
            QueueFree();
        }
    }

    private void UpdateFlame(Flame flame, float delta)
    {
        var offset = GetFlameOffset(flame);
        flame.Fire.SetCenter(offset);

        if (delta <= 0)
        {
            return;
        }

        var globalPosition = GlobalPosition + offset;
        var displacement = globalPosition - flame.PrevGlobalPosition;
        flame.PrevGlobalPosition = globalPosition;

        // The noise pattern stays in place in the world, so the fire evolves and trails behind
        var halfSize = flame.Fire.Size.X / 2;
        flame.Fire.AddNoiseOffset((displacement / halfSize).LimitLength(MaxDriftSpeed * delta));

        // The air blows the flame back, against the smoothed movement
        var weight = 1 - Exp(-LeanSmoothing * delta);
        flame.SmoothedVelocity = flame.SmoothedVelocity.Lerp(displacement / delta, weight);
        flame.Fire.SetLean(-flame.SmoothedVelocity / SpeedForMaxLean * Fire.MaxLean);
    }

    /// <summary>
    /// Position of the flame relative to the display. The display does not rotate,
    /// so the point inside the host is rotated by the host rotation.
    /// </summary>
    private Vector2 GetFlameOffset(Flame flame)
    {
        return flame.LocalPoint.Rotated(_hostNode.GlobalRotation);
    }

    private sealed class Flame(Fire fire, Vector2 localPoint)
    {
        public Fire Fire { get; } = fire;

        /// <summary>
        /// Point inside the host, in local coordinates of the host.
        /// </summary>
        public Vector2 LocalPoint { get; } = localPoint;

        public Vector2 PrevGlobalPosition { get; set; }
        public Vector2 SmoothedVelocity { get; set; }
        public Tween? Tween { get; set; }
        public bool IsBurningOut { get; set; }
    }

}

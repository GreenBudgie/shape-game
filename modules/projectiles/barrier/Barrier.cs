public partial class Barrier : RigidBody2D, IProjectile<Barrier>
{
    private static readonly PackedScene Scene = GD.Load<PackedScene>("uid://cgm0yfj1g1own");

    private static readonly StringName ProgressMaskParam = "progress";
    private static readonly NodePath ProgressMaskShaderParam = ShaderParameter(ProgressMaskParam);

    private static readonly Vector2 LeftCornerPosition = new(-225, 0);
    private static readonly Vector2 RightCornerPosition = new(225, 0);

    [Export]
    private AudioStream _appearSound = null!;
    
    [Export]
    private AudioStream _disappearSound = null!;

    private Sprite2D _sprite = null!;
    private ShaderMaterial _spriteMaterial = null!;
    private GpuParticles2D _particles = null!;
    private Vector2 _initialPosition;

    public Barrier Node => this;

    public static Barrier Create()
    {
        var node = Scene.Instantiate<Barrier>();
        return node;
    }

    public void Prepare(ShotContext context)
    {
        RotationDegrees = RandomUtils.DeltaRange(0, 15);
        
        _particles = GetNode<GpuParticles2D>("GPUParticles2D");
        _particles.Emitting = false;
        
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _spriteMaterial = (ShaderMaterial)_sprite.Material;
        _spriteMaterial.SetShaderParameter(ProgressMaskParam, 0f);

        const float yOffset = 250f;
        GlobalPosition += new Vector2(0, -yOffset);
    }

    private Glow _glow = null!;
    private uint _initialCollisionLayer;
    private uint _initialCollisionMask;
    
    public override void _Ready()
    {
        _initialPosition = GlobalPosition;
        _initialCollisionLayer = CollisionLayer;
        _initialCollisionMask = CollisionMask;
        CollisionLayer = 0;
        CollisionMask = 0;
        
        _glow = Glow.AddGlow(_sprite, addChild: false);
        _glow.Sprite.Scale = new Vector2(0, 1);
        _glow.SetColor(ColorScheme.LightBlueGreen)
            .SetStrength(1)
            .SetRadius(30);

        AddChild(_glow);

        var player = Player.FindPlayer();
        if (player != null)
        {
            PlaySpawnBeamEffectForPlayer(player);
        }

        PlayAppearEffect();
    }

    private Beam? _leftBeam;
    private Beam? _rightBeam;
    private float _animationProgress;

    public override void _Process(double delta)
    {
        UpdateBeams();
    }

    public override void _PhysicsProcess(double delta)
    {
        RetainPosition();
        RetainRotation();
    }

    private void UpdateBeams()
    {
        if (_leftBeam == null || _rightBeam == null)
        {
            return;
        }

        var player = Player.FindPlayer();
        if (player == null)
        {
            return;
        }

        if (IsInstanceValid(_leftBeam))
        {
            UpdateBeamFromTo(_leftBeam, LeftCornerPosition);
        }

        if (IsInstanceValid(_rightBeam))
        {
            UpdateBeamFromTo(_rightBeam, RightCornerPosition);
        }
    }
    
    
    private void UpdateBeamFromTo(Beam beam, Vector2 toPosition)
    {
        var globalPos = ToGlobal(toPosition * _animationProgress);
    
        var actualPlayer = Player.FindPlayer();
        if (actualPlayer == null)
        {
            beam.SetTo(globalPos);
            return;
        }

        beam.SetFromTo(actualPlayer.GetGlobalNosePosition(), globalPos);
    }

    private void RetainPosition()
    {
        const float force = 0.1f;
        var direction = _initialPosition - GlobalPosition;
        var speed = GlobalPosition.DistanceSquaredTo(_initialPosition);
        ApplyCentralForce(speed * force * direction);
    }

    private void RetainRotation()
    {
        // TODO code is stolen from EnemyRectangle
        var rotationDegrees = RotationDegrees;
        if (IsZeroApprox(rotationDegrees))
        {
            return;
        }

        const float torqueByDegree = 100000f;
        var direction = rotationDegrees < 0 ? 1 : -1;
        var torque = Abs(rotationDegrees) * torqueByDegree;
        ApplyTorque(torque * direction);
    }

    private bool _isRemoving;
    private const float RemoveDuration = 0.25f;

    public void Remove()
    {
        if (_isRemoving)
        {
            return;
        }
        _isRemoving = true;
        
        SoundManager.Instance.PlayPositionalSound(this, _disappearSound)
            .RandomizePitchOffset(0.1f);

        CollisionLayer = 0;
        CollisionMask = 0;
        
        _particles.Reparent(ShapeGame.Instance);
        _particles.OneShot = true;
        _particles.Emitting = false;
        _particles.Finished += QueueFree;
        
        var tween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(_spriteMaterial, ProgressMaskShaderParam, 0f, RemoveDuration);
        tween.TweenProperty(_glow.Sprite, ScaleProperty, new Vector2(0, 1), RemoveDuration);

        tween.Finished += QueueFree;
    }

    private const float EffectStartupDuration = 0.15f;
    private const float EffectDuration = 0.3f;
    private const float EffectEndDuration = 0.15f;

    private void PlayAppearEffect()
    {
        SoundManager.Instance.PlayPositionalSound(this, _appearSound)
            .RandomizePitchOffset(0.1f);
        
        var tween = CreateTween()
            .SetParallel()
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(_spriteMaterial, ProgressMaskShaderParam, 1f, EffectDuration)
            .SetDelay(EffectStartupDuration);
        
        tween.TweenProperty(_glow.Sprite, ScaleProperty, Vector2.One, EffectDuration)
            .SetDelay(EffectStartupDuration);
        
        tween.Finished += OnAppear;
    }

    private void OnAppear()
    {
        _particles.Emitting = true;
        CollisionLayer = _initialCollisionLayer;
        CollisionMask = _initialCollisionMask;
    }

    private void PlaySpawnBeamEffectForPlayer(Player player)
    {
        _leftBeam = CreateAndAnimateBeam(player);
        _rightBeam = CreateAndAnimateBeam(player);

        var tween = CreateTween();
        tween.TweenMethod(
                Callable.From<float>(progress => _animationProgress = progress),
                0f,
                1f,
                EffectDuration
            )
            .SetDelay(EffectStartupDuration)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Quad);
    }

    private Beam CreateAndAnimateBeam(Player player)
    {
        var beam = Beam.Create()
            .SetFromTo(player.GetGlobalNosePosition(), GlobalPosition)
            .SetEnergy(0)
            .SetProgress(0)
            .SetOutlineThickness(100)
            .SetOutlineColor(ColorScheme.LightBlueGreen)
            .SetBeamColor(ColorScheme.LightBlueGreen.Lightened(0.5f));

        var tween = beam.CreateTween();
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 1, EffectStartupDuration)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(beam.ShaderMaterial, Beam.ProgressShaderParam, 0, EffectEndDuration)
            .SetDelay(EffectDuration)
            .SetEase(Tween.EaseType.In)
            .SetTrans(Tween.TransitionType.Quad);
        tween.Finished += () => beam.QueueFree();
        ShapeGame.Instance.AddChild(beam);

        return beam;
    }
}
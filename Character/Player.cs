using System.Collections.Generic;
using ShapeGame.Common;
using ShapeGame.Projectile.Player.DoubleBolt;

namespace ShapeGame.Character;

public partial class Player : ShapeCastCharacterBody2D
{
    private static readonly PackedScene DoubleBoltProjectileScene =
        GD.Load<PackedScene>("res://Projectile/Player/DoubleBolt/double_bolt.tscn");

    private const int CornerDistance = 28;

    private const float PrimaryFireDelay = 0.4f;

    /**
     * The player's tilt will never go above this value.
     */
    [ExportGroup("Tilt")] [Export(PropertyHint.Range, "0,40,1,or_greater")]
    private double _maxTiltDegrees = 22;

    /**
     * How rapidly the player will tilt when it is moved horizontally. Higher = faster.
     */
    [Export(PropertyHint.Range, "0,1,0.01,or_greater")]
    private double _tiltIncreaseFactor = 0.1;

    /**
     * How fast the player's tilt will decrease over time (arbitrary number). Higher = faster.
     */
    [Export(PropertyHint.Range, "1,20,0.1,or_greater")]
    private double _tiltDecreaseFactor = 7;

    /**
     * The lowest threshold of player's tilt in degrees. When this value is reached, the tilt is instantly set to 0.
     */
    [Export(PropertyHint.Range, "0.1,2,0.1,or_greater")]
    private double _rotationDegreesEpsilon = 0.3;

    private float _primaryFireTimer;

    public override void _Ready()
    {
        this.InitAttributes();
        base._Ready();
    }

    public override void _Process(double delta)
    {
        var windowCenter = new Vector2(
            GetViewport().GetWindow().Size.X / 2.0f,
            GetViewport().GetWindow().Size.Y / 2.0f
        );
        var mousePosition = GetViewport().GetMousePosition();
        var mouseDelta = mousePosition - windowCenter;
        GetViewport().WarpMouse(windowCenter);

        var prevPosition = Position;
        Velocity = mouseDelta * (float)(1 / delta);
        MoveAndSlide();

        var positionDelta = Position - prevPosition;
        var tiltDegrees = positionDelta.X * _tiltIncreaseFactor;
        RotationDegrees *= Clamp(1 - (float)(delta * _tiltDecreaseFactor), 0, 1);
        RotationDegrees = (float)Clamp(RotationDegrees + tiltDegrees, -_maxTiltDegrees, _maxTiltDegrees);
        if (Abs(RotationDegrees) <= _rotationDegreesEpsilon)
        {
            RotationDegrees = 0;
        }

        if ((int)Input.GetActionStrength("primary_fire") == 1 && _primaryFireTimer <= 0)
        {
            PrimaryFire();
        }

        if (_primaryFireTimer > 0)
        {
            _primaryFireTimer -= (float)delta;
        }
    }

    private Vector2 GetNosePosition()
    {
        return Position - new Vector2(0, CornerDistance).Rotated(Rotation);
    }

    private Vector2 GetLeftCornerPosition()
    {
        return Position + new Vector2(-CornerDistance, CornerDistance).Rotated(Rotation);
    }

    private Vector2 GetRightCornerPosition()
    {
        return Position + new Vector2(CornerDistance, CornerDistance).Rotated(Rotation);
    }

    private void PrimaryFire()
    {
        var leftBolt = DoubleBoltProjectileScene.Instantiate<DoubleBoltProjectile>();
        var rightBolt = DoubleBoltProjectileScene.Instantiate<DoubleBoltProjectile>();
        leftBolt.Position = GetLeftCornerPosition();
        rightBolt.Position = GetRightCornerPosition();
        var bolts = new List<DoubleBoltProjectile> { leftBolt, rightBolt };
        foreach (var bolt in bolts)
        {
            bolt.Rotation = Rotation;
            var moveVector = new Vector2(0, -3000).Rotated(Rotation);
            bolt.ApplyCentralImpulse(moveVector);
            GetParent().AddChild(bolt);
            _primaryFireTimer = PrimaryFireDelay;
        }
    }
}
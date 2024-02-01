using System.Collections.Generic;
using ShapeGame.Common;
using ShapeGame.Projectile.Player.DoubleBolt;

namespace ShapeGame.Character;

public partial class Player : ShapeCastCharacterBody2D
{

    [Scene("Projectile/Player/DoubleBolt/double_bolt")]
    private PackedScene _doubleBoltProjectileScene;

    private const float PrimaryFireDelay = 0.4f;
    private const float MaxTiltDegrees = 22;
    private const float TiltIncreaseFactor = 0.15f;
    private const float TiltDecreaseFactor = 0.0055f;
    private const float RotationDegreesEpsilon = 0.3f;

    private const int CornerDistance = 28;
    private const int HorizontalTiltReserve = 10;

    private float _minX, _maxX, _minY, _maxY;

    private float _primaryFireTimer;

    public override void _Ready()
    {
        this.InitAttributes();
        _minX = CornerDistance - HorizontalTiltReserve;
        _maxX = GetViewportRect().Size.X - CornerDistance + HorizontalTiltReserve;
        _minY = CornerDistance;
        _maxY = GetViewportRect().Size.Y - CornerDistance;
        base._Ready();
    }

    public override void _Process(double delta)
    {
        var mousePosition = GetViewport().GetMousePosition();
        var moveVector = mousePosition - Position;
        var tiltDegrees = moveVector.X * TiltIncreaseFactor;
        RotationDegrees *= (float) (TiltDecreaseFactor / delta);
        RotationDegrees = Clamp(RotationDegrees + tiltDegrees, -MaxTiltDegrees, MaxTiltDegrees);
        if (Abs(RotationDegrees) <= RotationDegreesEpsilon)
        {
            RotationDegrees = 0;
        }

        Position += moveVector;
        Position = Position.Clamp(new Vector2(_minX, _minY), new Vector2(_maxX, _maxY));

        if ((int) Input.GetActionStrength("primary_fire") == 1 && _primaryFireTimer <= 0)
        {
            PrimaryFire();
        }
        if (_primaryFireTimer > 0)
        {
            _primaryFireTimer -= (float) delta;
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
        var leftBolt = _doubleBoltProjectileScene.Instantiate<DoubleBoltProjectile>();
        var rightBolt = _doubleBoltProjectileScene.Instantiate<DoubleBoltProjectile>();
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
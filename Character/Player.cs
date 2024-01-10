using System;
using ShapeGame.Common;
using ShapeGame.Enemies;
using ShapeGame.Character.Bullet;

namespace ShapeGame.Character;

public partial class Player : MovingArea2D
{

    [Scene("Character/Bullet/player_bullet")]
    private PackedScene _bulletScene;

    private const float PrimaryFireDelay = 0.4f;
    private const float MaxTiltDegrees = 22;
    private const float TiltIncreaseFactor = 0.15f;
    private const float TiltDecreaseFactor = 0.0055f;
    private const float RotationDegreesEpsilon = 0.3f;

    private float _primaryFireTimer;

    public override void _Ready()
    {
        this.InitAttributes();
        base._Ready();
    }

    protected override void OnCollide(CollisionObject2D collider)
    {
        if (collider is Enemy)
        {
            collider.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        var moveVector = GetGlobalMousePosition() - Position;
        var tiltDegrees = moveVector.X * TiltIncreaseFactor;
        RotationDegrees *= (float) (TiltDecreaseFactor / delta);
        RotationDegrees = Clamp(RotationDegrees + tiltDegrees, -MaxTiltDegrees, MaxTiltDegrees);
        if (Abs(RotationDegrees) <= RotationDegreesEpsilon)
        {
            RotationDegrees = 0;
        }

        MoveAndCollide(moveVector);
        
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
        return Position - new Vector2(0, 28).Rotated(DegToRad(RotationDegrees));
    }

    private void PrimaryFire()
    {
        var bullet = _bulletScene.Instantiate<PlayerBullet>();
        bullet.Position = GetNosePosition();
        bullet.Rotation = Rotation * 2;
        GetParent().AddChild(bullet);
        _primaryFireTimer = PrimaryFireDelay;
    }

}
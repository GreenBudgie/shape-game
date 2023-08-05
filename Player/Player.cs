using Godot;
using ShapeGame.Common;
using ShapeGame.Enemy;
using ShapeGame.Player.Bullet;

namespace ShapeGame.Player;

public partial class Player : MovingArea2D
{

    private PackedScene _bulletScene;

    private const float PrimaryFireDelay = 0.4f;
    private const float MaxTiltDegrees = 22;
    private const float TiltIncreaseFactor = 0.15f;
    private const float TiltDecreaseFactor = 0.0055f;
    private const float RotationDegreesEpsilon = 0.3f;

    private float _primaryFireTimer;

    public override void _Ready()
    {
        base._Ready();
        
        _bulletScene = GD.Load<PackedScene>("res://Player/Bullet/player_bullet.tscn");
    }

    protected override void OnCollide(CollisionObject2D collider)
    {
        if (collider is IEnemy)
        {
            collider.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        var moveVector = GetGlobalMousePosition() - Position;
        var tiltDegrees = moveVector.X * TiltIncreaseFactor;
        RotationDegrees *= (float) (TiltDecreaseFactor / delta);
        RotationDegrees = Mathf.Clamp(RotationDegrees + tiltDegrees, -MaxTiltDegrees, MaxTiltDegrees);
        if (Mathf.Abs(RotationDegrees) <= RotationDegreesEpsilon)
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
        return Position - new Vector2(0, 28).Rotated(Mathf.DegToRad(RotationDegrees));
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
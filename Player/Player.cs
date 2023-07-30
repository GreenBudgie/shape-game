using System;
using Godot;
using ShapeGame.Common;
using ShapeGame.Player.Bullet;

namespace ShapeGame.Player;

public partial class Player : ShapeCastArea2D
{

    private PackedScene _bulletScene;

    private const float PrimaryFireDelay = 0.4f;
    private const float MaxTiltDegrees = 22;
    private const float TiltIncreaseFactor = 15f;
    private const float TiltDecreaseFactor = 150f;

    private float _primaryFireTimer = 0;

    public override void _Ready()
    {
        base._Ready();
        
        _bulletScene = GD.Load<PackedScene>("res://Player/Bullet/player_bullet.tscn");
    }

    public override void _Process(double delta)
    {
        var moveVector = GetGlobalMousePosition() - Position;
        var tiltDegrees = moveVector.X * TiltIncreaseFactor * delta;
        RotationDegrees *= (float) (TiltDecreaseFactor * delta);
        RotationDegrees = (float) Mathf.Clamp(RotationDegrees + tiltDegrees, -MaxTiltDegrees, MaxTiltDegrees);

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

    protected override Shape2D GetShape()
    {
        var collisionPolygon = GetNode<CollisionPolygon2D>("CollisionPolygon2D");
        var shape = new ConvexPolygonShape2D();
        shape.Points = collisionPolygon.Polygon;
        return shape;
    }
    
}
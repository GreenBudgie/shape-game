using Godot;
using System;
using ShapeGame.Common;

namespace ShapeGame.Player.Bullet;

public partial class PlayerBullet : MovingArea2D
{
    
    private const int Speed = 1000;
    
    public override void _Process(double delta)
    {
        var actualSpeed = Speed * delta;
        var moveVector = new Vector2(0, (float) -actualSpeed).Rotated(Rotation);
        MoveAndCollide(moveVector);
        if (Position.Y < -10)
        {
            QueueFree();
        }
    }

}

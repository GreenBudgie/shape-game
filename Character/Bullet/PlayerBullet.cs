using System;
using ShapeGame.Common;
using ShapeGame.Enemies;

namespace ShapeGame.Character.Bullet;

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

    protected override void OnCollide(CollisionObject2D collider)
    {
        if (collider is EnemyBullet)
        {
            Console.WriteLine("player bullet collide with enemy bullet");   
            QueueFree();
        }
        if (collider is Enemy)
        {
            QueueFree();
        }
    }
}

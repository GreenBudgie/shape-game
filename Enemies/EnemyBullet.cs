using System;
using ShapeGame.Character.Bullet;
using ShapeGame.Common;

namespace ShapeGame.Enemies;

public abstract partial class EnemyBullet : MovingArea2D
{
    protected override void OnCollide(CollisionObject2D collider)
    {
        if (collider is PlayerBullet)
        {
            Console.WriteLine("enemy bullet collide with player bullet");
            QueueFree();
        }
    }
}
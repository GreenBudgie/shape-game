using Godot;
using ShapeGame.Common;

namespace ShapeGame.Enemy.Square.Bullet;

public partial class EnemySquareBullet : MovingArea2D
{

    private Sprite2D _sprite;

    private const int Speed = 800;
    private const int RotationSpeed = 10;

    public override void _Ready()
    {
        base._Ready();
        
        _sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public override void _Process(double delta)
    {
        var actualSpeed = (float) (Speed * delta);
        var moveVector = new Vector2(0, actualSpeed).Rotated(Rotation);

        MoveAndCollide(moveVector);
        
        _sprite.Rotation += (float) (RotationSpeed * delta);
        if (Position.Y > GetViewportRect().Size.Y + 10)
        {
            QueueFree();
        }
    }
}
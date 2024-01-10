using Godot;
using ShapeGame.Common;
using ShapeGame.Character;

namespace ShapeGame.Enemies.Square.Bullet;

public partial class EnemySquareBullet : EnemyBullet
{

    [Node("Sprite2D")]
    private Sprite2D _sprite;

    private const int Speed = 800;
    private const int RotationSpeed = 10;

    public override void _Ready()
    {
        this.InitAttributes();
        base._Ready();
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
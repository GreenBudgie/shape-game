using Godot;
using ShapeGame.Enemy.Square.Bullet;

namespace ShapeGame.Enemy.Square;

public partial class EnemySquare : Enemy
{

    private PackedScene _bulletScene;
    
    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    public override void _Ready()
    {
        base._Ready();
        
        _bulletScene = GD.Load<PackedScene>("res://Enemy/Square/Bullet/enemy_square_bullet.tscn");
    }

    public override void _Process(double delta)
    {
        if (_fireTimer <= 0)
        {
            Fire();
            _fireTimer = FireDelay;
        }
        else
        {
            _fireTimer -= delta;
        }
    }

    private void Fire()
    {
        var bullet = _bulletScene.Instantiate<EnemySquareBullet>();
        bullet.Position = Position;
        GetParent().AddChild(bullet);
    }
}
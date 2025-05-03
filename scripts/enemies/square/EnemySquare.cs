using Common;
using Projectile.Enemy.Square;

namespace Enemies.Square;

public partial class EnemySquare : Enemy
{

    private static readonly PackedScene BulletScene = 
        GD.Load<PackedScene>("res://scenes/projectile/enemy/square/enemy_square_bullet.tscn");
    
    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    public override void _Ready()
    {
        this.InitAttributes();
        base._Ready();
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
        var bullet = BulletScene.Instantiate<EnemySquareBullet>();
        bullet.Position = Position;
        GetParent().AddChild(bullet);
    }
}
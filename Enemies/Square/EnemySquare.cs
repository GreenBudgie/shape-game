using ShapeGame.Common;
using ShapeGame.Enemies.Square.Bullet;

namespace ShapeGame.Enemies.Square;

public partial class EnemySquare : Enemy
{

    [Scene("Enemies/Square/Bullet/enemy_square_bullet")]
    private PackedScene _bulletScene;
    
    private const double FireDelay = 1;

    private double _fireTimer = FireDelay;

    public override float GetMaxHealth() => 10;

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
        var bullet = _bulletScene.Instantiate<EnemySquareBullet>();
        bullet.Position = Position;
        GetParent().AddChild(bullet);
    }
}
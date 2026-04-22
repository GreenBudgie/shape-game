using System.Linq;

[GlobalClass]
public partial class NearestEnemyEyeTargetStrategy : EyeTargetStrategy
{
    public override Vector2? GetTarget(Eye eye, Node2D owner)
    {
        var ownerPosition = owner.GlobalPosition;
        return EnemyManager.Instance.GetAliveEnemies()
            .MinBy(e => e.GlobalPosition.DistanceSquaredTo(ownerPosition))
            ?.GlobalPosition;
    }
}
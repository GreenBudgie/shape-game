using System.Linq;

[GlobalClass]
public partial class NearestEnemyEyeTargetStrategy : EyeTargetStrategy
{
    public override Vector2? GetTarget(EyesController controller)
    {
        var ownerPosition = controller.EyeOwner.GlobalPosition;
        return EnemyManager.Instance.GetAliveEnemies()
            .MinBy(e => e.GlobalPosition.DistanceSquaredTo(ownerPosition))
            ?.GlobalPosition;
    }
}
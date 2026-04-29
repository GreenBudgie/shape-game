[GlobalClass]
public partial class PlayerEyeTargetStrategy : EyeTargetStrategy
{
    public override Vector2? GetTarget(EyesController controller)
    {
        return Player.FindPlayer()?.GlobalPosition;
    }
}
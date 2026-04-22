[GlobalClass]
public partial class PlayerEyeTargetStrategy : EyeTargetStrategy
{
    public override Vector2? GetTarget(Eye eye, Node2D owner)
    {
        return Player.FindPlayer()?.GlobalPosition;
    }
}
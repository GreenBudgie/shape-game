[GlobalClass]
public abstract partial class EyeTargetStrategy : Resource
{

    public abstract Vector2? GetTarget(Eye eye, Node2D owner);

}
public class TetherComponentType : ComponentType
{
    public override Component CreateComponent(ISpawnable spawnable)
    {
        return new TetherComponent { Spawnable = spawnable };
    }

    public override bool Supports(ISpawnable spawnable)
    {
        return spawnable is { Node: RigidBody2D, IsProxy: false };
    }

    public override bool AllowMultipleCopies(ISpawnable spawnable)
    {
        return true;
    }
}
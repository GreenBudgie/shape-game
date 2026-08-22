public class BarrierModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://ccqbbyqj8akl0");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Barrier";

    public override string Description => "Generates a projectile barrier in front of the player";

    public override int Price => 15;

    public override Module CreateModule()
    {
        return new BarrierModule();
    }
}

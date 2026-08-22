public class BoltModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://d3af7qu2ct723");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Bolt";

    public override string Description => "Fast and precise projectile. Very reliable!";

    public override int Price => 5;

    public override Module CreateModule()
    {
        return new BoltModule();
    }

}

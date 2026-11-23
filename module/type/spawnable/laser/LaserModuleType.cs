public class LaserModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b17h6ybaal1ad");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Laser";

    public override string Description => "Fires a laser beam";

    public override int Price => 10;

    public override Module CreateModule()
    {
        return new LaserModule();
    }

}

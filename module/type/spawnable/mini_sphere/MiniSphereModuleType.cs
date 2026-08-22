public class MiniSphereModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://4p56hm2gcdfy");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Mini Sphere";

    public override string Description => "Many short-lived and inaccurate projectiles";

    public override int Price => 5;

    public override Module CreateModule()
    {
        return new MiniSphereModule();
    }

}

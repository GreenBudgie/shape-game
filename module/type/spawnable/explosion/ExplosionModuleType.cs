public class ExplosionModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://svcj5wyx72em");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Explosion";

    public override string Description => "An instant-trigger explosion!";

    public override int Price => 15;

    public override Module CreateModule()
    {
        return new ExplosionModule();
    }

}

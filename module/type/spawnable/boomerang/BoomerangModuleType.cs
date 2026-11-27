public class BoomerangModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://bi2fi31uep3g4");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Boomerang";

    public override string Description => "Piercing projectile that flies back at you. But, maybe, you better not catch it...";

    public override int Price => 10;

    public override Module CreateModule()
    {
        return new BoomerangModule();
    }

}

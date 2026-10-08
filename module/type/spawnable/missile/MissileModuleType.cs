public partial class MissileModuleType : SpawnableModuleType
{
    public override Texture2D Texture => GD.Load<Texture2D>("uid://bqj0jnjof6b6p");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Missile";

    public override string Description => "An auto-targeting, explosive projectile";

    public override int Price => 8;

    public override Module CreateModule()
    {
        return new MissileModule();
    }
}
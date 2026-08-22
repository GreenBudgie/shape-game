public class MineModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b4uocapwodajq");

    public override ModuleShape Shape => ModuleShapeRegistry.Triple;

    public override string Name => "Mine";

    public override string Description => "A mine";

    public override int Price => 15;

    public override Module CreateModule()
    {
        return new MineModule();
    }

}

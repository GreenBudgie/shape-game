public class YinYangModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://crjvvf61weo2p");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Yin-Yang";

    public override string Description => "A group of projectiles in perfect balance. Until one of them is destroyed...";

    public override int Price => 5;

    public override Module CreateModule()
    {
        return new YinYangModule();
    }

}

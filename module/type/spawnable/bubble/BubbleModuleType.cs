public class BubbleModuleType : SpawnableModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://gu2c4f0fwbfb");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Bubbles";

    public override string Description => "Soap bubbles. They actually can damage!";

    public override int Price => 4;

    public override Module CreateModule()
    {
        return new BubbleModule();
    }

}

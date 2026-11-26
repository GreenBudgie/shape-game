using System.Collections.Generic;

public class LessSpeedModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://duklxqdw6a613");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Less Speed";

    public override string Description => "Halves projectile speed";

    public override int Price => 10;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Top];

    public override Module CreateModule()
    {
        return new LessSpeedModule();
    }

}

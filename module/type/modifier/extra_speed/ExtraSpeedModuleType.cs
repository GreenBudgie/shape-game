using System.Collections.Generic;

public class ExtraSpeedModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://o47kvuptx0hi");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Extra Speed";

    public override string Description => "Doubles projectile speed";

    public override int Price => 10;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Top];

    public override Module CreateModule()
    {
        return new ExtraSpeedModule();
    }

}

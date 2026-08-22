using System.Collections.Generic;

public class PiercingModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://cctsssm2r3hvt");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Piercing";

    public override string Description => "The projectile pierces +1 additional enemy";

    public override int Price => 15;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right * 2];

    public override Module CreateModule()
    {
        return new PiercingModule();
    }

}

using System.Collections.Generic;

public class IgnitionModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://dgwf5xg8innm7");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Ignition";

    public override string Description => "Sets enemies on fire!";

    public override int Price => 10;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override Module CreateModule()
    {
        return new MassiveShotModule();
    }

}

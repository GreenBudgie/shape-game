using System.Collections.Generic;

public class TriggerModuleType : DelegatorModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://883a7s6u8piw");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Trigger";

    public override string Description => "Spawns another projectile when connected projectile is removed";

    public override int Price => 15;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Top * 2];

    public override HashSet<HexCoordinates> IncomingConnections => [HexCoordinates.Bottom];

    public override Module CreateModule()
    {
        return new TriggerModule();
    }

}

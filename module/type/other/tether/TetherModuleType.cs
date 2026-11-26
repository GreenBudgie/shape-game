using System.Collections.Generic;

public class TetherModuleType : DelegatorModuleType
{

    // TODO fix
    public override Texture2D Texture => GD.Load<Texture2D>("uid://d3af7qu2ct723");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Tether";

    public override string Description => "Makes two projectiles tethered";

    public override int Price => 15;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Top];

    public override HashSet<HexCoordinates> IncomingConnections => [HexCoordinates.Bottom];

    public override Module CreateModule()
    {
        return new TetherModule();
    }

}

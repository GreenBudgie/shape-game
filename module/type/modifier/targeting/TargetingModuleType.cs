using System.Collections.Generic;

public class TargetingModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://c26el6thnpdbh");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Targeting";

    public override string Description => "Selects the nearest enemy and accelerates the projectile towards it";

    public override int Price => 15;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right * 2];

    public override Module CreateModule()
    {
        return new TargetingModule();
    }

}

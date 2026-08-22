using System.Collections.Generic;

public class MassiveShotModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b18uxs8lf5pwj");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Massive Shot";

    public override string Description => "Makes projectile heavier while increasing its damage";

    public override int Price => 10;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override Module CreateModule()
    {
        return new MassiveShotModule();
    }

}

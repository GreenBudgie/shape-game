using System.Collections.Generic;

public class ExtraDamageModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://b8t5eahyf0qyr");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Extra Damage";

    public override string Description => "Provides additional damage at the cost of reload time";

    public override int Price => 8;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override Module CreateModule()
    {
        return new ExtraDamageModule();
    }

}

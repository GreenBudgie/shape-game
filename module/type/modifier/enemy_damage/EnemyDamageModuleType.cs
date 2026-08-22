using System.Collections.Generic;

public class EnemyDamageModuleType : ModifierModuleType
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://ckttb5ut61hfs");

    public override ModuleShape Shape => ModuleShapeRegistry.Single;

    public override string Name => "Enemy Damage";

    public override string Description => "+1% damage for every destroyed enemy";

    public override int Price => 8;

    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right];

    public override Module CreateModule()
    {
        return new EnemyDamageModule();
    }

}

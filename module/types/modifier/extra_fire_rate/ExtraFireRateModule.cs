using System.Collections.Generic;

public class ExtraFireRateModule : ModifierModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://bet5sg5obim30");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Extra Fire Rate";

    public override string Description => "Increases fire rate";

    public override int Price => 10;

    public override List<SpawnableStat> Stats => [
        new ReloadStat { ValuePercent = -50f },
    ];
    
    public override HashSet<HexCoordinates> Connections => [HexCoordinates.Right * 2];

}

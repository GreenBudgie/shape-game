using System.Collections.Generic;

public class TriggerModule : ModifierModule
{

    public override Texture2D Texture => GD.Load<Texture2D>("uid://byomadw2augn4");

    public override ModuleShape Shape => ModuleShapeRegistry.Double;

    public override string Name => "Trigger";

    public override string Description => "Spawns another projectile when connected projectile expires";

    public override int Price => 15;

    public override List<SpawnableStat> Stats => [];
    
    public override HashSet<HexCoordinates> OutgoingConnections => [HexCoordinates.Right * 2];
    
    public override HashSet<HexCoordinates> IncomingConnections => [HexCoordinates.Left];

    public override Color Color => ColorScheme.LightBlue;

    public override void Modify(SpawnableContext context)
    {
        
    }

}

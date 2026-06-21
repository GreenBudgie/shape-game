[GlobalClass]
public partial class ModuleConnection : Resource
{

    /**
     * Represents the tile in the shape. The first tile is top-left with index of 0. The last tile is bottom right.
     * Tiles are numbered left to right, then top to bottom (like we read).
     */
    [Export] public int TileNumber { get; private set; }
    
    [Export] public HexDirection Direction { get; private set; }

}
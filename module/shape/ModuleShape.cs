using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class ModuleShape : Resource
{
    public abstract Texture2D Texture { get; }

    public abstract List<HexCoordinates> Tiles { get; }

    private List<Vector2>? _tilePositions;
    private Vector2? _center;
    private List<Vector2>? _centeredTilePositions;
    private Bitmap? _bitmap;

    public List<Vector2> TilePositions
    {
        get
        {
            return _tilePositions ??= Tiles
                .Select(tile => tile.ToVector())
                .Append(Vector2.Zero)
                .ToList();
        }
    }

    public Vector2 Center
    {
        get
        {
            return _center ??= TilePositions.Center();
        }
    }
    
    public List<Vector2> CenteredTilePositions
    {
        get
        {
            return _centeredTilePositions ??= TilePositions
                .Select(position => position - Center)
                .ToList();
        }
    }
    
    public Bitmap Bitmap
    {
        get
        {
            if (_bitmap != null)
            {
                return _bitmap;
            }

            var bitmap = new Bitmap();
            bitmap.CreateFromImageAlpha(Texture.GetImage());
            
            _bitmap = bitmap;
            return bitmap;
        }
    }
    
    public int TileCount => Tiles.Count + 1;
    
}
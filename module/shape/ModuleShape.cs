using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class ModuleShape : Resource
{

    public static readonly Vector2 HexSize = new(72f * Sqrt(3), 144f);
    public static readonly Vector2 HexHalfSize = HexSize / 2f;
    
    public abstract Texture2D Texture { get; }

    /// <summary>
    /// Hexes of this figure. For now, expects the zero hex to be present at the top-left.
    /// </summary>
    public abstract List<HexCoordinates> Hexes { get; }

    private Dictionary<HexCoordinates, Vector2>? _pixelHexPositions;
    private Bitmap? _bitmap;
    
    /// <summary>
    /// On-screen positions of the hexes, relative to the top-left corner of the texture
    /// </summary>
    public Dictionary<HexCoordinates, Vector2> PixelHexPositions
    {
        get
        {
            return _pixelHexPositions ??= Hexes
                .ToDictionary(hex => hex, hex => hex.ToPixel() + HexHalfSize);
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
    
}
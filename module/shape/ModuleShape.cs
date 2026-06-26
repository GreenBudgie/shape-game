using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class ModuleShape : Resource
{
    public abstract Texture2D Texture { get; }

    /// <summary>
    /// Hexes of this figure
    /// </summary>
    public abstract List<HexCoordinates> Hexes { get; }

    private Vector2? _center;
    private Dictionary<HexCoordinates, Vector2>? _pixelHexPositions;
    private Bitmap? _bitmap;

    /// <summary>
    /// On-screen center of the sprite
    /// </summary>
    public Vector2 PixelCenter
    {
        get
        {
            return _center ??= Hexes.Select(h => h.ToPixel()).BoundsCenter();
        }
    }
    
    /// <summary>
    /// On-screen positions of the hexes
    /// </summary>
    public Dictionary<HexCoordinates, Vector2> PixelHexPositions
    {
        get
        {
            return _pixelHexPositions ??= Hexes
                .ToDictionary(hex => hex, hex => hex.ToPixel() - PixelCenter);
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
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public abstract partial class ModuleShape : Resource
{

    public const float TexturePadding = 8f;
    public const float Size = 144f;
    
    // Exported texture has rounded corners. After applying padding in godot, the pointy-top part of the hexagon
    // is a little shifted because of that. This factor accounts for this shift.
    private const float HeightCorrectionFactor = 3.094f;

    public static readonly Vector2 HexSize = new(Size / 2f * Sqrt(3), Size);
    private static readonly Vector2 CornerGap = HexSize / 2f + new Vector2(TexturePadding, TexturePadding - HeightCorrectionFactor);
    
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
                .ToDictionary(hex => hex, hex => hex.ToPixel() + CornerGap);
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
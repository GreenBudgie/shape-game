using System.Collections.Generic;
using System.Linq;

public abstract class ModuleShape
{

    public const float TexturePadding = 8f;
    public const float Size = 144f;
    
    // Exported texture has rounded corners. After applying padding in godot, the pointy-top part of the hexagon
    // is a little shifted because of that. This factor accounts for this shift.
    private const float HeightCorrectionFactor = 3.094f;

    public static readonly Vector2 HexSize = new(Size / 2f * Sqrt(3), Size);
    private static readonly Vector2 CornerGap = HexSize / 2f + new Vector2(TexturePadding, TexturePadding - HeightCorrectionFactor);
    
    public abstract Texture2D OutlineTexture { get; }
    public abstract Texture2D FillTexture { get; }

    /// <summary>
    /// Hexes of this figure. For now, expects the zero hex to be present at the top-left.
    /// </summary>
    public abstract List<HexCoordinates> Hexes { get; }

    private Dictionary<HexCoordinates, Vector2>? _pixelHexPositions;
    private Bitmap? _bitmap;
    private Vector2? _pixelSize;
    
    /// <summary>
    /// On-screen positions of the hexes, relative to the top-left corner of the texture
    /// </summary>
    public Dictionary<HexCoordinates, Vector2> PixelHexPositions
    {
        get
        {
            return _pixelHexPositions ??= Hexes.ToDictionary(hex => hex, GetVisualHexPosition);
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
            bitmap.CreateFromImageAlpha(FillTexture.GetImage());
            
            _bitmap = bitmap;
            return bitmap;
        }
    }

    /// <summary>
    /// Returns the size (in pixels) of this shape as a bounding box
    /// </summary>
    public Vector2 PixelSize
    {
        get
        {
            if (_pixelSize.HasValue)
            {
                return _pixelSize.Value;
            }

            var positions = PixelHexPositions.Values;
            var min = new Vector2(positions.Min(p => p.X), positions.Min(p => p.Y));
            var max = new Vector2(positions.Max(p => p.X), positions.Max(p => p.Y));

            _pixelSize = max - min + HexSize;
            return _pixelSize.Value;
        }
    }

    public static Vector2 GetVisualHexPosition(HexCoordinates hex)
    {
        return hex.ToPixel() + CornerGap;
    }
    
}
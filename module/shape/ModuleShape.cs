using System.Collections.Generic;
using System.Linq;

public abstract class ModuleShape
{

    public const float TexturePadding = 8f;
    public const float Size = 144f;
    
    // The rounded left and right corners shorten the exported hexagon.
    private const float WidthCorrectionFactor = 3.094f;

    public static readonly Vector2 HexSize = new(Size, Size / 2f * Sqrt(3));
    private static readonly Vector2 CornerGap = HexSize / 2f + new Vector2(TexturePadding - WidthCorrectionFactor, TexturePadding);
    
    public abstract Texture2D OutlineTexture { get; }
    public abstract Texture2D FillTexture { get; }

    /// <summary>
    /// Hexes of this figure. The zero hex is at the bottom-left of the texture.
    /// </summary>
    public abstract List<HexCoordinates> Hexes { get; }

    private Dictionary<HexCoordinates, Vector2>? _pixelHexPositions;
    private Dictionary<HexCoordinates, Vector2>? _centeredPixelHexPositions;
    private Vector2? _textureOrigin;
    private Bitmap? _bitmap;
    private Vector2? _pixelSize;
    
    /// <summary>
    /// On-screen positions of the hexes, relative to the top-left corner of the texture (useful for control nodes)
    /// </summary>
    public Dictionary<HexCoordinates, Vector2> PixelHexPositions
    {
        get
        {
            return _pixelHexPositions ??= Hexes.ToDictionary(hex => hex, GetVisualHexPosition);
        }
    }
    
    /// <summary>
    /// On-screen positions of hex centers, relative to the center of the texture (useful for sprite nodes)
    /// </summary>
    public Dictionary<HexCoordinates, Vector2> CenteredPixelHexPositions
    {
        get
        {
            return _centeredPixelHexPositions ??= Hexes.ToDictionary(
                hex => hex, 
                hex => GetVisualHexPosition(hex) - FillTexture.GetSize() / 2f
            );
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

    public Vector2 GetVisualHexPosition(HexCoordinates hex)
    {
        var origin = _textureOrigin ??= new Vector2(CornerGap.X, FillTexture.GetHeight() - CornerGap.Y);
        return origin + hex.ToPixel();
    }
    
}
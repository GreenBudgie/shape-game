using System.Collections.Generic;
using System.Collections.Immutable;

/**
 * Using cube coordinates for hexagons
 */
public readonly struct HexCoordinates(int q, int r, int s)
{

    public const int HexEdges = 6;
    
    /**
     * Represents top-left -> bottom-right axis
     */
    public int Q => q;

    /**
     * Represents left -> right axis
     */
    public int R => r;

    /**
     * Represents bottom-left -> top-right axis
     */
    public int S => s;

    public static readonly HexCoordinates Zero = new(0, 0, 0);

    public static readonly HexCoordinates Right = new(1, -1, 0);
    public static readonly HexCoordinates TopRight = new(1, -1, 0);
    public static readonly HexCoordinates TopLeft = new(0, -1, 1);
    public static readonly HexCoordinates Left = new(-1, 0, 1);
    public static readonly HexCoordinates BottomLeft = new(-1, 1, 0);
    public static readonly HexCoordinates BottomRight = new(0, 1, -1);

    public static readonly ImmutableList<HexCoordinates> Directions =
    [
        Right,
        TopRight,
        TopLeft,
        Left,
        BottomLeft,
        BottomRight
    ];

    public static HexCoordinates operator +(HexCoordinates left, HexCoordinates right)
    {
        return new HexCoordinates(left.Q + right.Q, left.R + right.R, left.S + right.S);
    }

    public static HexCoordinates operator -(HexCoordinates left, HexCoordinates right)
    {
        return new HexCoordinates(left.Q - right.Q, left.R - right.R, left.S - right.S);
    }

    public static HexCoordinates operator *(HexCoordinates coordinates, int scale)
    {
        return new HexCoordinates(coordinates.Q * scale, coordinates.R * scale, coordinates.S * scale);
    }

    public static HexCoordinates operator /(HexCoordinates coordinates, int divisor)
    {
        return new HexCoordinates(coordinates.Q / divisor, coordinates.R / divisor, coordinates.S / divisor);
    }

    /**
     * Produces a closed loop of HexCoordinates, forming a hexagon of specified radius, starting from bottom-left
     * and going counter-clockwise
     */
    public static IEnumerable<HexCoordinates> Ring(int radius)
    {
        var hex = BottomLeft * radius;
        for (var i = 0; i < HexEdges; i++)
        {
            for (var j = 0; j < radius; j++)
            {
                yield return hex;
                hex += Directions[i];
            }
        }
    }

    /**
     * Similar to Ring, but produces a filled hexagon, spiraling around from the center
     */
    public static IEnumerable<HexCoordinates> Spiral(int radius)
    {
        yield return Zero;

        for (var i = 0; i < radius; i++)
        {
            foreach (var ringCoordinates in Ring(i))
            {
                yield return ringCoordinates;
            }
        }
    }
    
}
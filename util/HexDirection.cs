using System;

public enum HexDirection
{
    
    TopLeft,
    TopRight,
    Left,
    Right,
    BottomLeft,
    BottomRight
    
}

public static class HexDirectionExtensions
{

    public static float Radians(this HexDirection direction)
    {
        return direction switch
        {
            HexDirection.TopLeft => 2 * Pi / 3,
            HexDirection.TopRight => Pi / 3,
            HexDirection.Left => Pi,
            HexDirection.Right => 0,
            HexDirection.BottomLeft => 4 * Pi / 3,
            HexDirection.BottomRight => 5 * Pi / 3,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    
    public static float Degrees(this HexDirection direction)
    {
        return direction switch
        {
            HexDirection.TopLeft => 120,
            HexDirection.TopRight => 60,
            HexDirection.Left => 180,
            HexDirection.Right => 0,
            HexDirection.BottomLeft => 240, 
            HexDirection.BottomRight => 300,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
    
}
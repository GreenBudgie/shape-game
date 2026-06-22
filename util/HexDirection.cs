public enum HexDirection
{
    Right = 0,
    TopRight = 1,
    TopLeft = 2,
    Left = 3,
    BottomLeft = 4,
    BottomRight = 5
}

public static class HexDirectionExtensions
{
    public static float Radians(this HexDirection direction)
    {
        return (float)direction * Pi / 3;
    }

    public static float Degrees(this HexDirection direction)
    {
        return (float)direction * 60;
    }
    
    public static Vector2 ToVector(this HexDirection direction)
    {
        return Vector2.FromAngle(direction.Radians());
    }

    public static Vector2 ToVector(this HexDirection direction, float length)
    {
        return Vector2.FromAngle(direction.Radians()) * length;
    }

    public static HexDirection Rotated(this HexDirection direction, int steps)
    {
        return (HexDirection)(((int)direction + steps % 6 + 6) % 6);
    }

    public static HexDirection Clockwise(this HexDirection direction)
    {
        return direction.Rotated(-1);
    }

    public static HexDirection Counterclockwise(this HexDirection direction)
    {
        return direction.Rotated(1);
    }
}
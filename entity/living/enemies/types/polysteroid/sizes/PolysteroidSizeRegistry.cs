using System.Collections.Generic;

public static class PolysteroidSizeRegistry
{

    public static readonly List<PolysteroidSize> Sizes = [];

    public static readonly SmallPolysteroidSize Small = new();
    public static readonly MediumPolysteroidSize Medium = new();
    public static readonly BigPolysteroidSize Big = new();
    public static readonly HugePolysteroidSize Huge = new();

}
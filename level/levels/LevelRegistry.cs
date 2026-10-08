using System.Collections.Generic;
using System.Linq;

public static class LevelRegistry
{

    public static readonly List<Level> Levels = [];

    public static readonly Level1 Level1 = new();
    public static readonly Level2 Level2 = new();
        
    private static readonly Dictionary<int, Level> LevelByNumber = Levels
        .GroupBy(level => level.Number)
        .ToDictionary(
            group => group.Key,
            group => group.Single()
        );
    
    public static Level GetLevel(int number)
    {
        return LevelByNumber[number];
    }

}
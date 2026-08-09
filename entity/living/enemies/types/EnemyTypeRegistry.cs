using System.Collections.Generic;

public static class EnemyTypeRegistry
{
    
    public static readonly List<EnemyType> Types = [];

    public static readonly EnemyTypeSquare Square = new();
    public static readonly EnemyTypeRhombus Rhombus = new();
    public static readonly EnemyTypeRectangle Rectangle = new();
    public static readonly EnemyTypePolysteroid Polysteroid = new();

}
using System.Collections.Generic;
using System.Linq;

public partial class SpawnableManager : Node
{

    public static readonly StringName SpawnablesGroupName = "spawnables";

    public static SpawnableManager Instance { get; private set; } = null!;

    public SpawnableManager()
    {
        Instance = this;
    }
    
    public IEnumerable<ISpawnable<Node2D>> GetSpawnables()
    {
        return GetTree().GetNodesInGroup(SpawnablesGroupName).Cast<ISpawnable<Node2D>>();
    }
    
    public int GetSpawnablesCount()
    {
        return GetTree().GetNodeCountInGroup(SpawnablesGroupName);
    }

}
public enum SpawnableContextState
{
    
    /// <summary>
    /// Context has just been created. Spawnable is not yet produced (thus null), context is fully modifiable and
    /// will be changed in the future.
    /// </summary>
    New,

    /// <summary>
    /// Spawnable is produced and prepared to be spawned, but not yet added to the tree. Context modifications are NOT
    /// allowed. It is NOT allowed to add stats, but IS allowed to change the spawnable itself.
    /// </summary>
    Preparing,
    
    /// <summary>
    /// Spawnable has been added to the tree. Context modifications are not allowed, spawning it again is not allowed.
    /// </summary>
    Spawned,
    
}
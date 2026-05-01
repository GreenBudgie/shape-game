public interface ISpawnableModifier
{

    /// <summary>
    /// Modifies the context of a spawnable. Called after stats are calculated,
    /// but before spawnable has entered the tree
    /// </summary>
    public void Modify(SpawnableContext context);

}
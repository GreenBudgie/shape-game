public abstract class ModifierModuleType : ModuleType, ISpawnableModifier
{
    public virtual void Modify(SpawnableContext context)
    {
    }

    public override Color Color => ColorScheme.Yellow;
}
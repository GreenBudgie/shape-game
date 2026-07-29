using System.Collections.Generic;

public abstract partial class ModifierModule : Module, ISpawnableModifier
{
    public virtual void Modify(SpawnableContext context)
    {
    }

    public override Color Color => ColorScheme.Yellow;
}
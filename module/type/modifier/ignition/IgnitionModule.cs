using System.Linq;

public partial class IgnitionModule : ModifierModule
{

    public override ModuleType Type => ModuleTypeRegistry.Ignition;

    public override void Modify(SpawnableContext context)
    {
        if (context.IsModifierTypeApplied<IgnitionModule>())
        {
            return;
        }

        // TODO
    }

}

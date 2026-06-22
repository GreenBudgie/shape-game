#if TOOLS
[Tool]
public partial class HexSelectorPlugin : EditorPlugin
{
    private HexSelectorInspectorPlugin _inspector = null!;

    public override void _EnterTree()
    {
        _inspector = new HexSelectorInspectorPlugin();
        AddInspectorPlugin(_inspector);
    }

    public override void _ExitTree()
    {
        RemoveInspectorPlugin(_inspector);
    }
}
#endif

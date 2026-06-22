#if TOOLS
[Tool]
public partial class HexSelectorInspectorPlugin : EditorInspectorPlugin
{
    private const int Radius = 3;

    public override bool _CanHandle(GodotObject @object) => true;

    public override bool _ParseProperty(
        GodotObject @object,
        Variant.Type type,
        string name,
        PropertyHint hintType,
        string hintString,
        PropertyUsageFlags usageFlags,
        bool wide)
    {
        if (type != Variant.Type.Object || hintString != nameof(HexCoordinatesArray))
        {
            return false;
        }

        AddPropertyEditor(name, new HexSelectorProperty(Radius, HexSelectorMode.Resource));
        return true;
    }
}
#endif

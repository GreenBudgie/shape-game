#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Tool]
public partial class HexSelectorInspectorPlugin : EditorInspectorPlugin
{
    private const BindingFlags Flags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly Dictionary<string, HexSelectorAttribute> Attributes = Scan();

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
        if (!Attributes.TryGetValue(name, out var attribute))
        {
            return false;
        }

        AddPropertyEditor(name, new HexSelectorProperty(attribute.Radius));
        return true;
    }

    private static Dictionary<string, HexSelectorAttribute> Scan()
    {
        var map = new Dictionary<string, HexSelectorAttribute>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray()!; }

            foreach (var type in types)
            {
                if (type == null) continue;
                foreach (var member in type.GetMembers(Flags))
                {
                    if (member is not (FieldInfo or PropertyInfo)) continue;
                    var attribute = member.GetCustomAttribute<HexSelectorAttribute>();
                    if (attribute != null) map[member.Name] = attribute;
                }
            }
        }
        return map;
    }
}
#endif

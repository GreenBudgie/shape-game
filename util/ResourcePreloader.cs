using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Loads resources at the start of the game instead of on first use, so their loading does not cause hitches
/// in the middle of the game. New classes are picked up automatically, nothing has to be registered.
/// <br/><br/>
/// Covers two ways resources are loaded in the project:
/// <list type="bullet">
/// <item>Static fields like <c>private static readonly PackedScene Scene = GD.Load(...)</c>. They are initialized
/// when their class is used for the first time, so the static initializers of all such classes are run up front.</item>
/// <item>Properties like <c>public override PackedScene Scene => GD.Load(...)</c> of objects kept in static fields,
/// like the types in registries. Such properties are read once.</item>
/// </list>
/// </summary>
public static class ResourcePreloader
{

    private const BindingFlags StaticFields =
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>
    /// Resources loaded through properties. Godot keeps a loaded resource in its cache only while something
    /// references it, so without these references they could be unloaded and loaded from disk again on first use.
    /// </summary>
    private static readonly List<Resource> LoadedResources = [];

    public static void PreloadStaticResources()
    {
        // Open generic classes have no static fields of their own until their type arguments are known
        var types = typeof(ResourcePreloader).Assembly.GetTypes()
            .Where(type => !type.ContainsGenericParameters)
            .ToList();

        foreach (var type in types)
        {
            if (HasStaticResourceField(type))
            {
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }
        }

        foreach (var type in types)
        {
            foreach (var field in type.GetFields(StaticFields))
            {
                LoadResourceProperties(field);
            }
        }
    }

    private static bool HasStaticResourceField(Type type)
    {
        return type.GetFields(StaticFields).Any(field => typeof(Resource).IsAssignableFrom(field.FieldType));
    }

    /// <summary>
    /// Reads the resource properties of the object kept in the static field.
    /// Only plain C# objects are read. Godot objects have lots of engine properties returning resources,
    /// and reading them would load nothing of the project.
    /// </summary>
    private static void LoadResourceProperties(FieldInfo field)
    {
        if (typeof(GodotObject).IsAssignableFrom(field.FieldType))
        {
            return;
        }

        var properties = GetResourceProperties(field.FieldType);
        if (properties.Count == 0)
        {
            return;
        }

        // Reading the field runs the static initializer of its class, like any first use would
        var value = field.GetValue(null);
        if (value == null)
        {
            return;
        }

        foreach (var property in properties)
        {
            if (property.GetValue(value) is Resource resource)
            {
                LoadedResources.Add(resource);
            }
        }
    }

    private static List<PropertyInfo> GetResourceProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.CanRead
                               && property.GetIndexParameters().Length == 0
                               && typeof(Resource).IsAssignableFrom(property.PropertyType))
            .ToList();
    }

}

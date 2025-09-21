using System;
using System.Collections.Generic;
using System.Linq;

public partial class ModuleManager : Node
{
    public static ModuleManager Instance { get; private set; } = null!;

    public static readonly List<Module> Modules =
        ResourceSearcher.FindResourcesRecursively<Module>("res://modules/types");

    private static readonly Dictionary<Type, Module> ModuleByType = Modules
        .GroupBy(m => m.GetType())
        .ToDictionary(
            group => group.Key,
            group => group.Single()
        );

    public ModuleManager()
    {
        Instance = this;
    }

    public static T GetModule<T>() where T : Module
    {
        return (T)ModuleByType[typeof(T)];
    }
}
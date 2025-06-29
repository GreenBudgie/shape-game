using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ResourceSearcher
{

    public static List<T> FindResourcesInDirectory<T>(string directory) where T : Resource
    {
        var files = DirAccess.GetFilesAt(directory);
        var resourceFiles = files.Where(fileName => fileName.EndsWith(".tres"));
        var result = new List<T>();
        foreach (var file in resourceFiles)
        {
            var resource = GD.Load($"{directory}/{file}");

            if (resource is T requiredResource)
            {
                result.Add(requiredResource);
            }
        }

        return result;
    }

}
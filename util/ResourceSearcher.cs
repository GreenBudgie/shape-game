using System.Collections.Generic;
using System.Linq;
using Godot;

public static class ResourceSearcher
{
    
    /// <summary>
    /// Finds specified resource types in directories inside the given path. Does not search for resources
    /// in the path itself, only in included directories (not recursive). So, the structure should look like this:
    /// 
    /// <ul>
    ///     <li>path
    ///         <ul>
    ///             <li>dir1
    ///                 <ul>
    ///                     <li>type1.tres</li>
    ///                     <li>type2.tres</li>
    ///                 </ul>
    ///             </li>
    ///             <li>dir2
    ///                 <ul>
    ///                     <li>type3.tres</li>
    ///                 </ul>
    ///             </li>
    ///         </ul>
    ///     </li>
    /// </ul>
    /// </summary>
    /// <param name="path">Base directory path</param>
    /// <typeparam name="T">Resource to search for</typeparam>
    /// <returns>List of found resources of type T</returns>
    public static List<T> FindInnerResources<T>(string path) where T : Resource
    {
        var directories = DirAccess.GetDirectoriesAt(path);
        var result = new List<T>();
        foreach (var directory in directories)
        {
            result.AddRange(FindResourcesInDirectory<T>($"{path}/{directory}"));
        }

        return result;
    }
    
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
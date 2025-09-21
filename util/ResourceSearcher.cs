using System;
using System.Collections.Generic;
using System.IO;
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
            var subDirPath = Path.Combine(path, directory);
            result.AddRange(FindResourcesInDirectory<T>(subDirPath));
        }

        return result;
    }

    /// <summary>
    /// Recursively finds all resources of type <typeparamref name="T"/> in the specified directory and its subdirectories.
    /// </summary>
    /// <typeparam name="T">The type of resource to find, must inherit from <see cref="Resource"/>.</typeparam>
    /// <param name="path">The directory path to start the search, relative to 'res://'.</param>
    /// <returns>A list of resources of type <typeparamref name="T"/> found in the directory and its subdirectories.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided path is invalid or inaccessible.</exception>
    public static List<T> FindResourcesRecursively<T>(string path) where T : Resource
    {
        if (string.IsNullOrEmpty(path) || !DirAccess.DirExistsAbsolute(path))
        {
            throw new ArgumentException($"Invalid or inaccessible directory path: {path}", nameof(path));
        }

        var result = new List<T>();
        result.AddRange(FindResourcesInDirectory<T>(path));

        var dirAccess = DirAccess.Open(path);
        if (dirAccess == null)
        {
            GD.PushWarning($"Failed to open directory: {path}");
            return result;
        }

        foreach (var directory in dirAccess.GetDirectories())
        {
            var subDirPath = Path.Combine(path, directory);
            result.AddRange(FindResourcesRecursively<T>(subDirPath));
        }

        return result;
    }

    /// <summary>
    /// Finds all resources of type <typeparamref name="T"/> in the specified directory (non-recursive).
    /// </summary>
    /// <typeparam name="T">The type of resource to find, must inherit from <see cref="Resource"/>.</typeparam>
    /// <param name="directory">The directory path to search, relative to 'res://'.</param>
    /// <returns>A list of resources of type <typeparamref name="T"/> found in the directory.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided directory path is invalid or inaccessible.</exception>
    public static List<T> FindResourcesInDirectory<T>(string directory) where T : Resource
    {
        if (string.IsNullOrEmpty(directory) || !DirAccess.DirExistsAbsolute(directory))
        {
            throw new ArgumentException($"Invalid or inaccessible directory path: {directory}", nameof(directory));
        }

        var result = new List<T>();
        var dirAccess = DirAccess.Open(directory);
        if (dirAccess == null)
        {
            GD.PushWarning($"Failed to open directory: {directory}");
            return result;
        }

        var files = dirAccess.GetFiles().Where(fileName => fileName.EndsWith(".tres"));
        foreach (var file in files)
        {
            var resourcePath = Path.Combine(directory, file);
            var resource = GD.Load(resourcePath);
            if (resource is T requiredResource)
            {
                result.Add(requiredResource);
            }
        }

        return result;
    }
}
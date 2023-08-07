using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ShapeGame.Common;

/// <summary>
/// Allows to load the scene by the specified path instead of calling <see cref="GD.Load"/>.
/// It is not required to use "res://" prefix and ".tscn" suffix in the path.
/// Invoke <see cref="Node2DExtension.InitAttributes"/> at the first line of <see cref="Node._Ready"/> method
/// for this to work.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SceneAttribute : Attribute
{

    private const string Prefix = "res://";
    private const string Postfix = ".tscn";

    public SceneAttribute(string path)
    {
        var scenePathBuilder = new StringBuilder();
        if (!path.StartsWith(Prefix))
        {
            scenePathBuilder.Append(Prefix);
        }

        scenePathBuilder.Append(path);

        if (!path.EndsWith(Postfix))
        {
            scenePathBuilder.Append(Postfix);
        }

        ScenePath = scenePathBuilder.ToString();
    }
    
    public string ScenePath { get; private set; }
    
}
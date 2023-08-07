using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ShapeGame.Common;

/// <summary>
/// Allows to get the specified child node without calling <see cref="Node.GetChild"/>.
/// Providing node name in constructor is not required if it can be inferred from the field name.
/// Invoke <see cref="Node2DExtension.InitAttributes"/> at the first line of <see cref="Node._Ready"/> method
/// for this to work.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class NodeAttribute : Attribute
{

    public string NodeName { get; }

    public NodeAttribute(string nodeName = null)
    {
        NodeName = nodeName;
    }
    
}
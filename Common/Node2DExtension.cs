using System;
using System.Reflection;

namespace ShapeGame.Common;

public static class Node2DExtension
{

    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    
    public static void InitAttributes(this Node node)
    {
        foreach (var field in node.GetType().GetFields(Flags))
        {
            InitNodeAttribute(node, field);
        }
    }
    
    private static void InitNodeAttribute(Node node, FieldInfo field)
    {
        var attribute = Attribute.GetCustomAttribute(field, typeof(NodeAttribute));
        if (attribute is not NodeAttribute nodeAttribute)
        {
            return;
        }

        var nodeName = nodeAttribute.NodeName ?? GetCorrectedNodeName(field.Name);

        var childNode = node.GetNode(nodeName);
        field.SetValue(node, childNode);
    }

    private static string GetCorrectedNodeName(string rawNodeName)
    {
        var result = rawNodeName;
        if (rawNodeName.StartsWith("_"))
        {
            result = result[1..];
        }

        if (rawNodeName.EndsWith("Node"))
        {
            result = result[..^4];
        }

        return char.ToUpper(result[0]) + result[1..];
    }
    
}
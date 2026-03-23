public static class NodeExtensions
{

    public static void AddChildDeferred(this Node node, Node child)
    {
        Callable.From(() => node.AddChild(child)).CallDeferred();
    }
    
}
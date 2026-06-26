public static class NodeExtensions
{

    public static void AddChildDeferred(this Node node, Node child)
    {
        Callable.From(() => node.AddChild(child)).CallDeferred();
    }

    public static void SetCenterGlobalPosition(this Control control, Vector2 position)
    {
        control.Position = position - control.Size / 2;
    }
    
}
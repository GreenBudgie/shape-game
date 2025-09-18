public static class CallableUtils
{

    public static void CallNextFrame(this Callable callable, SceneTree tree)
    {
        tree.Connect(
            SceneTree.SignalName.ProcessFrame,
            callable,
            (uint)GodotObject.ConnectFlags.OneShot
        );
    }
    
    public static void CallNextPhysicsFrame(this Callable callable, SceneTree tree)
    {
        tree.Connect(
            SceneTree.SignalName.PhysicsFrame,
            callable,
            (uint)GodotObject.ConnectFlags.OneShot
        );
    }
    
}
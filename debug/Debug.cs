public partial class Debug : Node
{

    public static readonly bool Enabled = true;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("debug_timescale"))
        {
            if (IsEqualApprox(Engine.TimeScale, 0.2f))
            {
                Engine.TimeScale = 1f;
            }
            else
            {
                Engine.TimeScale = 0.2f;
            }
        }
        
        if (Input.IsActionJustPressed("debug_restart"))
        {
            ShapeGame.Instance.Restart();
        }
    }

    public static bool IsDebugButtonJustPressed()
    {
        if (!Enabled)
        {
            PrintDebugNotEnabledError();
            return false;
        }
        
        return Input.IsActionJustPressed("debug_button");
    }
    
    public static bool IsDebugButtonPressed()
    {
        if (!Enabled)
        {
            PrintDebugNotEnabledError();
            return false;
        }
        
        return Input.IsActionPressed("debug_button");
    }

    public static void PrintDebugNotEnabledError()
    {
        GD.PrintErr("Debug is used in a non-debug environment");
    }

}